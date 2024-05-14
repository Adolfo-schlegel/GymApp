#include <SPI.h>
#include <MFRC522.h>   

#define SS_PIN 10
#define RST_PIN 9

const int buzzer = 7; 
const int relay = 8;

MFRC522 mfrc522(SS_PIN, RST_PIN);


void setup()  {
  Serial.begin(9600);    
  pinMode(relay, OUTPUT);    
  while (!Serial);    
  SPI.begin();      
  mfrc522.PCD_Init();   
  delay(1000);
  mfrc522.PCD_DumpVersionToSerial(); 
}
void loop() {
  if(Serial.available() > 0 )
  {        
    int myCharacter = Serial.read();
    
    if(myCharacter == 'E')
    {      
      tone(buzzer, 500, 500); 
        
      digitalWrite(relay, HIGH);
      
      unsigned long startTime = millis();      
      while (millis() - startTime < 3000) {
       // Espera durante 3 segundos sin bloquear el programa
      }       
      
      digitalWrite(relay, LOW);              
    }
    if(myCharacter == 'X')
    {
      tone(buzzer, 500, 500);

      unsigned long startTime = millis();
      while (millis() - startTime < 500) {
       // Espera durante 3 segundos sin bloquear el programa
      }
      
      tone(buzzer, 100, 1000);

      while (millis() - startTime < 1000) {
       // Espera durante 3 segundos sin bloquear el programa
      }
    }
  }

  if ( ! mfrc522.PICC_IsNewCardPresent()) {
    return;
  }
  if ( ! mfrc522.PICC_ReadCardSerial()) {     
    return;
  }
  mfrc522.PICC_DumpToSerial(&(mfrc522.uid));
  
  Serial.println(ReadMfrc());
}
String ReadMfrc() {
    String content;
    for (byte i = 0; i < mfrc522.uid.size; i++) {
        content += (mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ");
        content += String(mfrc522.uid.uidByte[i], HEX);
    }
    return content;
}
