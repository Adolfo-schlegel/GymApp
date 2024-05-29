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
  // Manejar la lectura de tarjetas RFID
  handleRFID();

  // Manejar los comandos desde el puerto serial
  handleSerialCommands();
}

void handleRFID() {
  // Verificar si hay una tarjeta RFID presente
  if (mfrc522.PICC_IsNewCardPresent()) {
    // Leer la tarjeta y realizar las operaciones necesarias
    if (mfrc522.PICC_ReadCardSerial()) {
      mfrc522.PICC_DumpToSerial(&(mfrc522.uid));
      Serial.println(ReadMfrc());
    }
  }
}

void handleSerialCommands() {
  // Verificar si hay datos disponibles en el puerto serial
  if (Serial.available() > 0) {
    // Leer el comando serial
    int myCharacter = Serial.read();
    
    // Realizar acciones basadas en el comando recibido
    if (myCharacter == 'E') {
      // Acción para el comando 'E'
      performActionE();
    } else if (myCharacter == 'X') {
      // Acción para el comando 'X'
      performActionX();
    }
  }
}

void performActionE() {
  // Realizar acciones asociadas con el comando 'E'
  
  tone(buzzer, 500, 500);
  digitalWrite(relay, HIGH);

  unsigned long startTime = millis();
  while (millis() - startTime < 3000) {}
   
  digitalWrite(relay, LOW);
}

void performActionX() {
  // Realizar acciones asociadas con el comando 'X'
  
  tone(buzzer, 500, 500);
  
  unsigned long startTime = millis();
  while (millis() - startTime < 500) {}
  
  tone(buzzer, 100, 1000);
  
  unsigned long startTime2 = millis();
  while (millis() - startTime2 < 1000) {}
}

String ReadMfrc() {
    String content;
    for (byte i = 0; i < mfrc522.uid.size; i++) {
        content += (mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ");
        content += String(mfrc522.uid.uidByte[i], HEX);
    }
    return content;
}
