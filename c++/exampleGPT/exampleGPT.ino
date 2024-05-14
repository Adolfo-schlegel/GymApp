#include <SPI.h>
#include <MFRC522.h>

#define SS_PIN 53
#define RST_PIN 49

MFRC522 mfrc522(SS_PIN, RST_PIN);

void setup() {
  Serial.begin(9600);
  SPI.begin();
  mfrc522.PCD_Init();   // Init MFRC522
  Serial.println(F("Scan PICC to see UID, SAK, type, and data blocks..."));
}

void loop() {
  // Reset the loop if no new card present on the sensor/reader. This saves the entire process when idle.
  if ( ! mfrc522.PICC_IsNewCardPresent()) {
    return;
  }

  // Select one of the cards
  if ( ! mfrc522.PICC_ReadCardSerial()) {
    return;
  }

  // Dump debug info about the card; PICC_HaltA() is automatically called
 // mfrc522.PICC_DumpToSerial(&(mfrc522.uid));
 // String content= "";
 // byte letter;
  Serial.print("Tag: UID:");
  for (byte i = 0; i < mfrc522.uid.size; i++) 
  {
     String content1 = mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ";
     String content2 = String(mfrc522.uid.uidByte[i], HEX);
     
     Serial.print(content1);
     Serial.print(content2);
  }
  Serial.println();
  mfrc522.PICC_HaltA();
}
