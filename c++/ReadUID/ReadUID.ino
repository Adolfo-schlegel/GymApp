/*
 * 
 * Typical pin layout used:
 * -----------------------------------------------------------------------------------------
 *             MFRC522      Arduino       Arduino   Arduino    Arduino          Arduino
 *             Reader/PCD   Uno/101       Mega      Nano v3    Leonardo/Micro   Pro Micro
 * Signal      Pin          Pin           Pin       Pin        Pin              Pin
 * -----------------------------------------------------------------------------------------
 * RST/Reset   RST          9             49         D9         RESET/ICSP-5     RST
 * SPI SS      SDA(SS)      10            53        D10        10               10
 * SPI MOSI    MOSI         11 / ICSP-4   51        D11        ICSP-4           16
 * SPI MISO    MISO         12 / ICSP-1   50        D12        ICSP-1           14
 * SPI SCK     SCK          13 / ICSP-3   52        D13        ICSP-3           15
 */
//Display
//int columna = 16;
//int fila = 2;
//int RS = 2;
//int E = 3;
//int D4 = 4;
//int D5 = 5;
//int D6= 6;
//int D7=7;
//LiquidCrystal lcd(RS,E,D4,D5,D6,D7); // inicializa la interfaz I2C del LCD 16x2

#include <SPI.h>
#include <MFRC522.h>
#include <LiquidCrystal.h>
#include <Wire.h>
#include <Servo.h>

//Arduino Mega
//#define RST_PIN 49      
//#define SS_PIN 53 
//const int servoPin = 2; 
//const int ledGreen = 3;
//const int ledRed = 4;
//const int buzzer = 5;

//Arduino Nano
     
#define SS_PIN 10
#define RST_PIN 9

const int servoPin = 2;
const int buzzer = 7; 
const int ledRed = 6;
const int ledGreen = 5;


Servo servoMotor;
MFRC522 mfrc522(SS_PIN, RST_PIN);  // Create MFRC522 instance

void setup()  {
  Serial.begin(9600);    // Initialize serial communications with the PC
  while (!Serial);    // Do nothing if no serial port is opened (added for Arduinos based on ATMEGA32U4)
  SPI.begin();      // Init SPI bus

  pinMode(ledGreen, OUTPUT);
  pinMode(ledRed, OUTPUT);
  //pinMode(servoPin, OUTPUT);
  
  servoMotor.attach(servoPin);
  mfrc522.PCD_Init();    // Init MFRC522
	delay(10);// Optional delay. Some board do need more time after init to be ready, see Readme
	
	mfrc522.PCD_DumpVersionToSerial();	// Show details of PCD - MFRC522 Card Reader details
  servoMotor.write(0);
}
void loop() {
  digitalWrite(ledRed, HIGH);
  if(Serial.available() > 0 )
  {        
    int myCharacter = Serial.read();
    
    if(myCharacter == 'E')
    {
      digitalWrite(ledRed, LOW);
      digitalWrite(ledGreen, HIGH); 
      tone(buzzer, 1000, 500); delay(500);            
      giroPeaje();            
      digitalWrite(ledGreen, LOW); 
    }
    if(myCharacter == 'X')
    {
      tone(buzzer, 500, 500); delay(500);
      tone(buzzer, 100, 1000); delay(1000);
    }
  }

	// Reset the loop if no new card present on the sensor/reader. This saves the entire process when idle.
	if ( ! mfrc522.PICC_IsNewCardPresent()) {
		return;
	}

	// Select one of the cards
	if ( ! mfrc522.PICC_ReadCardSerial()) {     
		return;
	}
   
  digitalWrite(ledRed, LOW);
	mfrc522.PICC_DumpToSerial(&(mfrc522.uid));// Dump debug info about the card;
  Serial.println(ReadMfrc());
}

String ReadMfrc() {
  String content= "";  
  for (byte i = 0; i < mfrc522.uid.size; i++) 
  {     
     String content1 = mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ";
     String content2 = String(mfrc522.uid.uidByte[i], HEX);
     content.concat(content1);
     content.concat(content2);           
  }
  return content;
}

void giroPeaje() {
  servoMotor.write(90);
  delay(2000);
  servoMotor.write(0);
}

//void animacion(String mensaje) {
// lcd.clear();
//  for (int i = 0; i < mensaje.length() + 16; i++) {
//    lcd.clear();
    // Muestra el mensaje desplazado
//    lcd.setCursor(max(0, 15 - i), 0);
//    lcd.print(mensaje.substring(max(0, i - 15)));
//    delay(100);
//  }
//  lcd.clear();
//}
