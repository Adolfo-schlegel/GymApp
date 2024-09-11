#include <SPI.h>
#include <MFRC522.h>   

#define SS_PIN 10
#define RST_PIN 9

const int buzzer = 7; 
const int relay = 8;
String lastUID = "";  // Variable to almacenar el último UID leído
bool cardDetected = false;  // Indicador para saber si una tarjeta ha sido detectada
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
  if (mfrc522.PICC_IsNewCardPresent() && mfrc522.PICC_ReadCardSerial()) {
    String currentUID = "";
    
    // Leer el UID y formatearlo correctamente
    for (byte i = 0; i < mfrc522.uid.size; i++) {
      currentUID += String(mfrc522.uid.uidByte[i], HEX);
      if (i < mfrc522.uid.size - 1) {
        currentUID += " ";  // Añadir espacio entre bytes
      }
    }

    currentUID.toUpperCase();  // Convertir a mayúsculas para legibilidad

    // Verificar si el UID es diferente al último leído
    if (currentUID != lastUID) {
      lastUID = currentUID;  // Actualizar el último UID leído
      cardDetected = true;   // Marcar que se detectó una tarjeta
      Serial.println("Card UID: " + currentUID);
      
      delay(1000);  // Evitar lecturas repetidas en menos de un segundo
    }
  }

  // Reiniciar cuando la tarjeta ya no está presente
  if (!mfrc522.PICC_IsNewCardPresent() && cardDetected) {
    lastUID = "";  // Restablecer el UID
    cardDetected = false;  // Reiniciar la detección
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
