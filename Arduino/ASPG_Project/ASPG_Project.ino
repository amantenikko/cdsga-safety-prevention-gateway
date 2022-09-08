#include <Wire.h>                                                         //Arduino library for I2C modules and devices
#include <HCMP3.h>                                                        //Arduino library for HCMODU0138
#include <LiquidCrystal_I2C.h>                                            //Arduino library for LCD I2C interface.
#include <Adafruit_MLX90614.h>                                            //Arduino library to support I2C Melexis MLX90614
#include <millisDelay.h>                                                  //Arduino library for Non-blocking flow of code.
#include <HCSR04.h>                                                       //Arduino library for HCSR04 Ultrasonic Sensor.
#include <Servo.h>                                                        //Arduino library for controlling servo motors.
#include <SPI.h>
#include <PN532_SPI.h>
#include <snep.h>
#include <NdefMessage.h>

#define pinTX 2
#define pinRX 3
#define pinEcho 4
#define pinTrig 5
#define pinServo 6
#define pumpMotor 7
#define palmSensor 8
//#define pin 9  - Used by PN532
#define pinNFC 10
//#define pin 11 - Used by PN532
//#define pin 12 - Used by PN532
//#define pin 13 - Used by PN532
#define pinVstrCntR 14
#define pinVstrCntL 15
#define pinLEDr 16
#define pinLEDg 17
//#define pin 18 - Can't Use due to I2C
//#define pin 19 - Can't Use due to I2C

#define tempAbsAdjusted 0.0
#define tempDistanceMax 2.5
#define tempDistanceMin 0.5
#define tempScanSize 3
#define tempScanMin 1

UltraSonicDistanceSensor distanceSensor(pinTrig, pinEcho);
LiquidCrystal_I2C lcd(0x27,16,2);
Adafruit_MLX90614 mlx = Adafruit_MLX90614();
HCMP3 hcmp3(pinRX, pinTX);
millisDelay waitLoop,waitPump,waitVoice,waitVstrCntR,waitVstrCntL;
Servo barrierBoom;

PN532_SPI pn532spi(SPI, pinNFC);
SNEP nfc(pn532spi);
uint8_t ndefBuf[128];

long waitPumpDuration = 1500;
//double new_emissivity = 1.00;
byte tempSymbol[] = { 0x0E,0x0A,0x0A,0x0E,0x1F,0x1F,0x0E,0x00 };
double tempScanCount[tempScanSize];
double tempAcquired = 0, tempAdjusted = 0, tempFever = 37.50;
double tempDistanceMax = 0.35, tempDistanceMin = 0.1;
int tempStep = 0,tempWXYZ,tempWX,tempX,tempYZ,tempZ;

String rawLine;

bool vstrCntRState;                                                       //Current state of the Right IR-Sensor
bool isVstrCntRTrigered;                                                  //Anti bouncing for the Right IR-Sensor
bool lastVstrCntRState;                                                   //Previous state of the Right IR-Sensor
bool vstrCntLState;                                                       //Current state of the Left IR-Sensor
bool isVstrCntLTrigered;                                                  //Anti bouncing for the Left IR-Sensor
bool lastVstrCntLState;                                                   //Previous state of the Left IR-Sensor
bool isVstrCntRTrigeredDenied;
bool isVstrCntFull = false;

int bBoomPos = 0;

void setup() {
  lcd.init();                                                             //Initialize LCD
  lcd.createChar(0, tempSymbol);                                          //Create Custom Char for temperature symbol
  lcd.home();
  lcd.print("Starting...");
  lcd.backlight();
  
  hcmp3.reset();
  hcmp3.volume(10);                                                       //Set Volume of Speaker at 10
  hcmp3.play(2,35);                                                       //Play "System Initializing" audio

  pinMode(pinLEDg, OUTPUT);
  pinMode(pinLEDr, OUTPUT);
  digitalWrite(pinLEDr,HIGH);
  digitalWrite(pinLEDg,HIGH);

  pinMode(pumpMotor, OUTPUT);
  pinMode(palmSensor, INPUT);

  pinMode(pinVstrCntR, INPUT);
  pinMode(pinVstrCntL, INPUT);
  vstrCntRState = digitalRead(pinVstrCntR);
  lastVstrCntRState = vstrCntRState;
  vstrCntLState = digitalRead(pinVstrCntL);
  lastVstrCntLState = vstrCntLState;

  delay(700);                                                            //Wait 0.7 seconds
  
  clearRowLCD(1);
  lcd.setCursor(0,1);
  lcd.print("Serial");                                                   //Display a text on LCD
  Serial.begin(9600);                                                    //Start serial communication at 9600bps
  delay(700);                                                            //Wait 0.7 seconds
  if (!Serial) {
    clearRowLCD(0);
    printError();
  }

  clearRowLCD(1);
  lcd.setCursor(0,1);
  lcd.print("IR sensor");                                                //Display a text on LCD
  delay(700);                                                            //Wait 0.7 seconds
//  if ((digitalRead(pinVstrCntR) == LOW) || 
//      (digitalRead(pinVstrCntL) == LOW)) {
//    clearRowLCD(0);
//    printError();
//  }

  clearRowLCD(1);
  lcd.setCursor(0,1);
  lcd.print("MLX sensor");                                               //Display a text on LCD
  delay(700);                                                            //Wait 0.7 seconds
  if (!mlx.begin()) {
    clearRowLCD(0);
    printError();
  }
  //Serial.print("Current emissivity = ");
  //Serial.println(mlx.readEmissivity());
  //mlx.writeEmissivity(new_emissivity);
  //Serial.println(mlx.readEmissivity());

  clearRowLCD(1);
  lcd.setCursor(0,1);
  lcd.print("Servo Motor");                                              //Display a text on LCD
  delay(700);                                                            //Wait 0.7 seconds
  barrierBoom.attach(pinServo);
  barrierBoom.write(90);
  delay(700);                                                            //Wait 0.7 seconds
    
  lcd.clear();
  lcd.print("Device Ready");
  hcmp3.play(2,37);                                                      //Play sound fx audio
  
  delay(700);                                                            //Wait 0.7 seconds
  lcd.home();
  lcd.print("Temperature:");

  tempScanCount[0] = 0;
  waitLoop.start(350);                                                   //Start Start timer with n milliseconds
  waitVstrCntR.start(100);                                               //Start Start timer with n milliseconds
  waitVstrCntL.start(100);                                               //Start Start timer with n milliseconds
}

void loop() {
  if (Serial.available() > 0) {                                          //Check if there is a Message from Software
    rawLine = Serial.readString();                                       //Read and save the Message
    if (rawLine == "vCountf1") {
      isVstrCntFull = true;
    }else if (rawLine == "vCountf0") {
      isVstrCntFull = false;
    }else if (rawLine.startsWith("tempF")) {
      tempFever = rawLine.substring(5).toDouble();
      hcmp3.play(2,33);                                                  //Play "Settings has been changed" audio
    }else if (rawLine.startsWith("tempA")) {
      tempAdjusted = rawLine.substring(5).toDouble();
      hcmp3.play(2,33);                                                  //Play "Settings has been changed" audio
    }else if (rawLine.startsWith("pumpD")) {
      waitPumpDuration = rawLine.substring(5).toInt();
      hcmp3.play(2,33);                                                  //Play "Settings has been changed" audio
    }else if (rawLine.startsWith("tempH")) {
      tempDistanceMax = rawLine.substring(5).toInt();
      hcmp3.play(2,33);                                                  //Play "Settings has been changed" audio
    }else if (rawLine.startsWith("tempL")) {
      tempDistanceMin = rawLine.substring(5).toInt();
      hcmp3.play(2,33);                                                  //Play "Settings has been changed" audio
    }
  }

  readVisitor();                                                         //Call readVisitor() method every loop
  
  if (waitPump.justFinished()) digitalWrite(pumpMotor,LOW);              //Stop Sanitizer Afte n ms

  if (waitLoop.justFinished()) {
    if (isVstrCntFull) {                                                 //Do if isVstrCntFull is True
      digitalWrite(pinLEDr,LOW);
      digitalWrite(pinLEDg,LOW);
      lcd.clear();
      lcd.print("Full Vstr Count");
    }else if ((tempStep == 0) && (digitalRead(palmSensor) == LOW) &&
    (distanceSensor.measureDistanceCm() >= tempDistanceMin) &&
    (distanceSensor.measureDistanceCm() <= tempDistanceMax)) {
      if (tempScanCount[0] == 0) {
        tempScanCount[0] = mlx.readObjectTempC();                        //read temperature
        if (tempScanCount[0] >= 45 && tempScanCount[0] <= 20) tempScanCount[0] = 0;
        tempScanCount[1] = 0;
        hcmp3.play(2,39);                                                //Play sound fx audio
      }else if (tempScanCount[1] == 0) {
        tempScanCount[1] = mlx.readObjectTempC();                        //read temperature
        if (tempScanCount[1] >= 45 && tempScanCount[1] <= 20) tempScanCount[1] = 0;
        tempScanCount[2] = 0;
        hcmp3.play(2,39);                                                //Play sound fx audio
      }else if (tempScanCount[2] == 0) {
        tempScanCount[2] = mlx.readObjectTempC();                        //read temperature
        if (tempScanCount[2] >= 45 && tempScanCount[2] <= 20) tempScanCount[2] = 0;
        hcmp3.play(2,39);                                                //Play sound fx audio
      }else if (abs(tempScanCount[0] - tempScanCount[1]) < tempScanMin &&
               abs(tempScanCount[1] - tempScanCount[2]) < tempScanMin && 
               abs(tempScanCount[2] - tempScanCount[0]) < tempScanMin) { //Compare each sample if less than tempScanMin
        
        tempAcquired = (tempScanCount[0] + tempScanCount[1] +
                        tempScanCount[2]) / 3;                           //Set average of temperature samples
        tempAcquired += (tempAdjusted + tempAbsAdjusted);                //Apply Settings adjustment to aquired temp
        
        digitalWrite(pumpMotor,HIGH);                                    //Start Sanitizer
        waitPump.start(waitPumpDuration);                                //Set Sanitizer duration
        
        tempWXYZ = tempAcquired * 100;                                   //temperature value, wxyz digit (in wx.yz dgC)
        tempWX = tempWXYZ / 100;                                         //temperature value, wx digit (in wx.yz dgC)
        tempX = tempWX % 10;                                             //temperature value, x digit (in wx.yz dgC)
        tempYZ = tempWXYZ % 100;                                         //temperature value, yz digit (in wx.yz dgC)
        tempZ = tempYZ % 10;                                             //temperature value, z digit (in wx.yz dgC)
        tempAcquired = tempWXYZ * 0.01;                                  //temperature value, wx.yz digit (in wx.yz dgC)
        
        Serial.print("1;");                                              //Send data to_
        Serial.println(tempAcquired);                                    //_ASPG Software
        
        if (tempAcquired >= tempFever) {                                 //Change LED indicator to red if True
          digitalWrite(pinLEDr,HIGH);
          digitalWrite(pinLEDg,LOW);
        }
        else {                                                           //Change LED indicator to Green
          digitalWrite(pinLEDr,LOW);
          digitalWrite(pinLEDg,HIGH);
        }
        
        lcd.setCursor(8,1);
        lcd.write(0);
        lcd.print(tempWX);
        lcd.print(".");
        if((tempYZ/10) == 0) lcd.print(0);
        lcd.print(tempYZ);
        lcd.print("\337C");
        
        tempStep++;
        hcmp3.play(2,32);                                                //Play "Your Temperature is" audio
        waitVoice.start(1760);
      }else{
        tempScanCount[0] = 0;
        hcmp3.play(2,38);                                                //Play audio
      }
    }
    waitLoop.repeat();                                                   //Rerun timer
  }
  
  if(waitVoice.justFinished()) {
    tempStep++;
    if(tempStep == 2) {
      speak(tempWX,true,true);
    }else if(tempStep == 3) {
      if((tempX!=0)&&(tempWX>19)) {
        speak(tempX, false, false);
      }else{
        waitVoice.start(10);                                             //Start Start timer with n milliseconds
      }
    }else if(tempStep == 4) {
      hcmp3.play(2,30);                                                  //Play "Point" audio
      waitVoice.start(303);
    }else if(tempStep == 5) {
      speak(tempYZ, true, true);
    }else if(tempStep == 6) {
      if ((tempZ != 0)&&((tempYZ < 9)||(tempYZ > 19))) {
        speak(tempZ, false, false);
      }else{
        waitVoice.start(10);                                             //Start Start timer with n milliseconds
      }
    }else if(tempStep == 7) {
      hcmp3.play(2,31);                                                  //Play "Degree Celsius" audio
      waitVoice.start(1952);                                             //Start Start timer with n milliseconds
    }else if(tempStep == 8) {
      lcd.clear();
      lcd.print("Scanning...info");
      int msgSize = nfc.read(ndefBuf, sizeof(ndefBuf), 3000);            //Read Ndef message from phone for 3s
      if (msgSize > 0) {
          NdefMessage msg  = NdefMessage(ndefBuf, msgSize);
          NdefRecord record = msg.getRecord(0);
          int payloadLength = record.getPayloadLength();
          byte payload[payloadLength];
          record.getPayload(payload);
  
          int startChar = 0;        
          if (record.getTnf() == TNF_WELL_KNOWN && record.getType() == "T") { // text message
            startChar = payload[0] + 1;
          } else if (record.getTnf() == TNF_WELL_KNOWN && record.getType() == "U") { // URI
            startChar = 1;
          }
          String payloadAsString = "";
          for (int c = startChar; c < payloadLength; c++) {
            payloadAsString += (char)payload[c];
          }
          Serial.print("3;");                                             //Send VstrInfo to_
          Serial.println(payloadAsString);                                //_ASPG Software for auto fill-up
      }
      if (tempAcquired >= tempFever) {
        barrierBoom.write(0);
      }
      else {
        barrierBoom.write(90);
      }
      waitVoice.start(500);
    }else{
      Serial.flush();
      lcd.clear();
      lcd.print("Temperature");
      hcmp3.reset();
      
      digitalWrite(pinLEDr,HIGH);
      digitalWrite(pinLEDg,HIGH);
      
      waitLoop.start(500);
      waitVstrCntR.start(100);
      waitVstrCntL.start(100);
      tempScanCount[0] = 0;
      tempScanCount[1] = 0;
      tempScanCount[2] = 0;
      tempStep = 0;
    }
  }
}

void readVisitor(){
  vstrCntRState = digitalRead(pinVstrCntR);
  vstrCntLState = digitalRead(pinVstrCntL);

  if (waitVstrCntR.justFinished()) {
    if ((vstrCntRState != lastVstrCntRState) && (vstrCntRState == LOW)) {
      if (isVstrCntLTrigered) {
        isVstrCntRTrigered = false;
        isVstrCntLTrigered = false;
        Serial.print("2;");                                              //Send decrement count signal_
        Serial.println("-1");                                            //_to ASPG Software
      }else if (isVstrCntRTrigered){
        barrierBoom.write(90);
        isVstrCntRTrigered = false;
      }
      else{
        barrierBoom.write(0);
        isVstrCntRTrigered = true;
      }
    }
    lastVstrCntRState = vstrCntRState;
    waitVstrCntR.repeat();
  }

  if (waitVstrCntL.justFinished()) {
    if ((vstrCntLState != lastVstrCntLState) && (vstrCntLState == LOW)) {
      if (isVstrCntRTrigered) {
        isVstrCntRTrigered = false;
        isVstrCntLTrigered = false;
        Serial.print("2;");                                              //Send increment count signal_
        Serial.println("1");                                             //_to ASPG Software
      }
      else{
        isVstrCntLTrigered = true;
      }
    }
    lastVstrCntLState = vstrCntLState;
    waitVstrCntL.repeat();
  }
}

void speak(int val,bool spkTens, bool spkZero) {
  if(spkTens){
    if((val <= 9)&&(spkZero)){hcmp3.play(2,29); waitVoice.start(548);}   //Play "zero" audio
    if(val == 10){hcmp3.play(2,10);waitVoice.start(406);}                //Play "ten" audio
    if(val == 11){hcmp3.play(2,11);waitVoice.start(558);}                //Play "eleven" audio
    if(val == 12){hcmp3.play(2,12);waitVoice.start(404);}                //Play "twelve" audio
    if(val == 13){hcmp3.play(2,13);waitVoice.start(630);}                //Play "thirteen" audio
    if(val == 14){hcmp3.play(2,14);waitVoice.start(655);}                //Play "fourteen" audio
    if(val == 15){hcmp3.play(2,15);waitVoice.start(635);}                //Play "fifteen" audio
    if(val == 16){hcmp3.play(2,16);waitVoice.start(755);}                //Play "sixteen" audio
    if(val == 17){hcmp3.play(2,17);waitVoice.start(795);}                //Play "seventeen" audio
    if(val == 18){hcmp3.play(2,18);waitVoice.start(627);}                //Play "eighteen" audio
    if(val == 19){hcmp3.play(2,19);waitVoice.start(702);}                //Play "nineteen" audio
    if((val >= 20)&&(val <= 29)){hcmp3.play(2,20);waitVoice.start(456);} //Play "twenty" audio
    if((val >= 30)&&(val <= 39)){hcmp3.play(2,21);waitVoice.start(451);} //Play "thirty" audio
    if((val >= 40)&&(val <= 49)){hcmp3.play(2,22);waitVoice.start(495);} //Play "fourty" audio
    if((val >= 50)&&(val <= 59)){hcmp3.play(2,23);waitVoice.start(493);} //Play "fifty" audio
    if((val >= 60)&&(val <= 69)){hcmp3.play(2,24);waitVoice.start(622);} //Play "sixty" audio
    if((val >= 70)&&(val <= 79)){hcmp3.play(2,25);waitVoice.start(629);} //Play "seventy" audio
    if((val >= 80)&&(val <= 89)){hcmp3.play(2,26);waitVoice.start(434);} //Play "eighty" audio
    if((val >= 90)&&(val <= 99)){hcmp3.play(2,27);waitVoice.start(604);} //Play "ninety" audio
  }else{
    if(val == 1){hcmp3.play(2,1);waitVoice.start(370);}                  //Play "one" audio
    if(val == 2){hcmp3.play(2,2);waitVoice.start(377);}                  //Play "two" audio
    if(val == 3){hcmp3.play(2,3);waitVoice.start(391);}                  //Play "three" audio
    if(val == 4){hcmp3.play(2,4);waitVoice.start(364);}                  //Play "four" audio
    if(val == 5){hcmp3.play(2,5);waitVoice.start(383);}                  //Play "five" audio
    if(val == 6){hcmp3.play(2,6);waitVoice.start(508);}                  //Play "six" audio
    if(val == 7){hcmp3.play(2,7);waitVoice.start(395);}                  //Play "seven" audio
    if(val == 8){hcmp3.play(2,8);waitVoice.start(364);}                  //Play "eight" audio
    if(val == 9){hcmp3.play(2,9);waitVoice.start(345);}                  //Play "nine" audio
  }
}

void clearRowLCD(int row) {               
  lcd.setCursor(0,row);
  for(int i = 0; i < 16; i++) {
          lcd.print(" ");
  }
}

void printError(){
  lcd.setCursor(0,0);
  lcd.print("Error...");                                                 //Display a text on LCD
  while (1) {
    lcd.noDisplay();                                                     //turn off the display
    delay(250);
    lcd.display();                                                       //turn on the display
    delay(500);
  }
}
