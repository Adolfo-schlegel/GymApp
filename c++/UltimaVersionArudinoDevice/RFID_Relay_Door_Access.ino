#include <SPI.h>
#include <MFRC522.h>   

#define SS_PIN 10
#define RST_PIN 9

const int buzzer = 7; 
const int relay = 8;
char lastUID[16] = "";  // Para almacenar el último UID leído como una cadena de caracteres
bool cardDetected = false;  // Indicador para saber si una tarjeta ha sido detectada
MFRC522 mfrc522(SS_PIN, RST_PIN);

unsigned long lastReadTime = 0;  // Almacenar el tiempo de la última lectura
const unsigned long debounceDelay = 1000;  // Retardo para evitar lecturas consecutivas (1 segundo)

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
  // Manejar los comandos desde el puerto serial
  handleSerialCommands();
  // Manejar la lectura de tarjetas RFID
  handleRFID();
}
void handleRFID() {
  // Verificar si hay una tarjeta RFID presente y si ha pasado suficiente tiempo desde la última lectura
  if (mfrc522.PICC_IsNewCardPresent() && mfrc522.PICC_ReadCardSerial() && (millis() - lastReadTime > debounceDelay)) {
    char currentUID[16] = "";  // Buffer para el UID actual

    // Leer el UID y formatearlo en hexadecimal directamente en mayúsculas
    for (byte i = 0; i < mfrc522.uid.size; i++) {
      sprintf(&currentUID[i * 3], "%02X", mfrc522.uid.uidByte[i]);  // Almacenar cada byte en HEX en mayúsculas
      if (i < mfrc522.uid.size - 1) {
        currentUID[i * 3 + 2] = ' ';  // Añadir espacio entre bytes
      }
    }

    // Verificar si el UID es diferente al último leído
    if (strcmp(currentUID, lastUID) != 0) {     
      strcpy(lastUID, currentUID);  // Actualizar el último UID leído
      Serial.println("Card UID: " + String(currentUID));  // Enviar el UID por el puerto serial  

      lastReadTime = millis();  // Actualizar el tiempo de la última lectura
      cardDetected = true;  // Establecer que se ha detectado una tarjeta
      
      // Aquí podrías agregar código para ejecutar acciones como activar un relé, etc.
    }
  } 
  else if (cardDetected && !mfrc522.PICC_IsNewCardPresent()) {
    // Si no hay una tarjeta presente pero antes se había detectado una, resetear el último UID leído
    lastUID[0] = '\0';  // Borrar el último UID leído
    cardDetected = false;  // Indicar que ya no hay una tarjeta en el rango
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