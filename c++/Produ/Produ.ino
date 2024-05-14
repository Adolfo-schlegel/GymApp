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
        
      digitalWrite(relay, HIGH); delay(3000);        
      digitalWrite(relay, LOW);              
    }
    if(myCharacter == 'X')
    {
      tone(buzzer, 500, 500); delay(500);
      tone(buzzer, 100, 1000); delay(1000);
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
