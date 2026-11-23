\# TFG – Sistema de vuelo y realidad aumentada con drones



\## 1. Descripción del proyecto



Proyecto desarrollado en Unity para la visualización, monitorización y ejecución de escenarios de vuelo con drones mediante realidad aumentada.



El sistema permite trabajar con escenarios predefinidos y personalizados, visualizar la posición del dron, gestionar pruebas de vuelo y representar elementos virtuales con los que interactuar durante la experiencia.



El proyecto se encuentra actualmente preparado para trabajar con un dron GPS mediante la biblioteca `csDronLink`. Una de las líneas de evolución es incorporar drones sin GPS, como DJI RoboMaster Tello Talent.



\## 2. Tecnologías utilizadas



\- Unity 6.3 LTS (6000.3.18f1).

\- C#.

\- Biblioteca `csDronLink` para comunicación con el dron actual.

\- MAVLink para comunicación con el sistema de vuelo.

\- WebRTC para transmisión de vídeo.

\- Sistema de coordenadas GPS y conversión a coordenadas Unity.



\## 3. Estructura del proyecto



Los scripts se organizan por responsabilidades:



```text

Assets/Scripts/

│

├── Core/

│   ├── ConnectionConfig.cs

│   └── SceneManagement.cs

│

├── Drone/

│   ├── Connection/

│   ├── Control/

│   ├── Geofencing/

│   ├── Telemetry/

│   └── Video/

│

├── Coordinates/

│

├── Gameplay/

│   ├── Gates/

│   └── Gems/

│

├── Results/

│

├── Scenario/

│   ├── Persistence/

│   └── PresetScenarios/

│

└── UI/

&#x20;   ├── Map/

&#x20;   └── Scenario/

```



\### Core

Configuración global del proyecto y gestión de escenas.



\### Drone

Funcionalidades relacionadas con el dron:

\- Connection: gestión centralizada de la conexión.

\- Control: envío de órdenes y control de vuelo.

\- Geofencing: generación y envío de geocercas.

\- Telemetry: recepción y procesamiento de telemetría.

\- Video: visualización y transmisión de vídeo.



\### Coordinates

Conversión entre coordenadas GPS y coordenadas de Unity, así como alineación del mundo virtual.



\### Gameplay

Elementos y mecánicas de las partidas:

\- Puertas y obstáculos.

\- Gemas y coleccionables.

\- Temporizadores.

\- Colisiones.



\### Results

Gestión, cálculo y visualización de resultados y recorridos.



\### Scenario

Construcción y gestión de escenarios:

\- Escenarios predefinidos.

\- Escenarios personalizados.

\- Persistencia de escenarios guardados.



\### UI

Controladores de interfaz, menús, mapas y herramientas de creación de escenarios.



\## 4. Escenas principales



El proyecto utiliza cuatro escenas principales:



| Escena | Función |

|---|---|

| MenuScene | Menú principal y configuración de conexión y escenario. |

| OpenGameScene | Preparación y apertura de la experiencia. |

| GameScene | Ejecución del escenario y experiencia de vuelo. |

| ResultsScene | Visualización de resultados. |



\## 5. Escenarios disponibles



El proyecto dispone de los siguientes tipos de escenario:



\- \*\*DroneFlow:\*\* recorrido secuencial por niveles y objetivos.

\- \*\*Circuit:\*\* circuito de vuelo con puertas.

\- \*\*Easy:\*\* escenario de dificultad sencilla.

\- \*\*Hard:\*\* escenario de dificultad elevada.

\- \*\*Search:\*\* escenario de búsqueda de objetivos.

\- \*\*Custom:\*\* escenario creado por el usuario.



Los escenarios se gestionan mediante `ScenarioManager`, que delega su construcción en los scripts especializados.



\## 6. Sistema de coordenadas



Actualmente el proyecto utiliza coordenadas GPS como referencia geográfica.



La arquitectura se ha organizado para centralizar las conversiones:



\- `GPSCoordinateConverter`: realiza las conversiones entre GPS y Unity.

\- `WorldAnchor`: actúa como intermediario para los componentes que necesitan utilizar la referencia geográfica.



La conversión utiliza una referencia geográfica y representa las posiciones en el mundo Unity mediante los ejes X y Z.



\## 7. Conexión y comunicación con el dron



La comunicación con el dron se centraliza en `DroneConnectionManager`.



Este componente gestiona:

\- Verificación de conexión.

\- Conexión en simulación o producción.

\- Envío de órdenes de control.

\- Suscripciones y cancelación de telemetría.

\- Envío de escenarios y geocercas.



El resto de componentes utilizan esta interfaz sin acceder directamente a los objetos `Dron` de `csDronLink`.



\### Modos de conexión



\- \*\*Simulation:\*\* conexión al sistema SITL mediante TCP en `127.0.0.1:5763`.

\- \*\*Production:\*\* conexión mediante puerto COM.



La conexión de producción requiere disponer del dron y del hardware correspondiente.



Nota: la biblioteca actual no expone un método público de desconexión física. El método `Disconnect()` detiene las suscripciones de telemetría y limpia la referencia local al objeto.



\## 8. Estado actual



Se ha realizado una fase de refactorización para mejorar la organización y el mantenimiento del proyecto.



Los principales cambios han sido:

\- Separación de la lógica de construcción de escenarios.

\- Centralización de las conversiones GPS.

\- Centralización de la comunicación con el dron.

\- Organización de scripts por responsabilidades.



El funcionamiento en simulación se ha comprobado después de estos cambios.



\## 9. Líneas de desarrollo futuras



Las siguientes fases previstas son:



1\. Diseñar un sistema de coordenadas que permita trabajar con GPS y con posiciones locales de Unity.

2\. Incorporar soporte para drones sin GPS, inicialmente DJI RoboMaster Tello Talent.

3\. Adaptar la interfaz para seleccionar el tipo de dron y gestionar sus estados de conexión.

4\. Definir las funcionalidades iniciales compatibles con el nuevo dron.

5\. Preparar un primer escenario de prueba, inicialmente basado en anillos y gemas.

6\. Integrar y probar la cámara del dron.

7\. Estudiar las posibilidades de control, calibración y limitaciones del hardware.



Estas funcionalidades todavía no forman parte de la implementación actual.

## 10. DJI RoboMaster Tello Talent en Windows

La conexión con el Tello se ha validado desde Python con MoreTelloLink. La integración con el menú de Unity está pendiente.

Para preparar el entorno, instala Git y Python 3.14 de 64 bits. Ejecuta estos comandos en PowerShell desde la raíz del proyecto, con acceso a Internet:

```powershell
.\Tools\Tello\setup.ps1
py -3.14 -m venv .\Tools\Tello\.venv
.\Tools\Tello\.venv\Scripts\python.exe -m pip install --only-binary=:all: djitellopy opencv-python pygame-ce
```

`setup.ps1` descarga la revisión fijada de MoreTelloLink en `Tools/Tello/MoreTelloLink`. El entorno `Tools/Tello/.venv` contiene `djitellopy` para la comunicación por UDP, `opencv-python` para las funciones de cámara y `pygame-ce`, que proporciona el módulo `pygame` compatible con Python 3.14 en Windows. Git ignora ambas carpetas locales.

Para comunicarse con el dron, el PC debe estar conectado a su Wi‑Fi `TELLO-...`. En ese modo, el Tello recibe comandos del SDK en `192.168.10.1:8889` por UDP.

