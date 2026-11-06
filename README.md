# Carnotaurus Rider (Realidad Mixta & Computación Física)

Simulador y videojuego de conducción en Unity 6 donde el vehículo es un Carnotauro de DinoWest equipado con montura, silla de montar y riendas, controlado mediante hardware físico basado en Arduino o teclado para pruebas.

## 👥 Equipo de Desarrollo (Universidad de Boyacá)

| Integrante | Correo Institucional | Rol Principal |
|---|---|---|
| Juan J. Plazas | jjplazas@uniboyaca.edu.co | Lead Gameplay & Vehicle Physics Architecture |
| K. J. Barreto | Kjbarreto@uniboyaca.edu.co | Physical Computing & Arduino Hardware Integration |
| David Rosas | davrosas@uniboyaca.edu.co | 3D Art, PBR Materials & Level Design |
| Laura Rivera | leriverap@uniboyaca.edu.co | Físicas: choque del árbol, vuelo del pájaro y pelota |

## 📸 Capturas del Proyecto

| Vista de Conducción (Pista de Pruebas) | Detalle del Carnotauro con Montura |
<img width="1600" height="900" alt="WhatsApp Image 2026-11-06 at 8 27 05 AM" src="https://github.com/user-attachments/assets/54dbf450-19e6-441d-8e3b-6e978b7fbe23" />

| Vista de Conducción | Detalle Montura |
<img width="1600" height="900" alt="WhatsApp Image 2026-11-06 at 8 27 15 AM" src="https://github.com/user-attachments/assets/60b31071-5443-4c79-9052-1bc813bd55e6" />


## ⚙️ Características Principales

**Física Vehicular de Criatura:**
- Control de masa e inercia animal (1200 kg) con Rigidbody.
- Sistema de tracción terrestre con alineación a pendientes del terreno.
- Balanceo visual del cuerpo (banking) en giros a alta velocidad.
- Pisotón y salto físico con detección de suelo (SphereCast).
- Demolición física de obstáculos (pirámides de cajas destructibles con rebote dinámico).

**Modelo 3D y Gráficos (DinoWest):**
- Carnotauro DW con montura completa: silla de cuero, peto, estribos, funda de rifle, cabezada y cadenas de riendas articuladas.
- Texturizado PBR: mapas de difusión en alta resolución, normales de escamas y reflejo de ojos.
- Animaciones integradas: Idle, Walk, Run, Roar y Attack_HornRam.

**Arquitectura de Entrada Dual (Doble Fuente):**
- Modo Teclado (Simulación): permite probar y calibrar sin hardware conectado.
- Modo Arduino Serial (Hardware Físico): comunicación USB de baja latencia (115200 baudios) en hilo de ejecución secundario (Thread), evitando caídas de frames en Unity.

**Cámara Dinámica y HUD:**
- Cámara de seguimiento en tercera persona con interpolación suave (DinoFollowCamera).
- Campo de visión dinámico (FOV) que se expande a alta velocidad o durante el turbo.
- Sacudida de cámara (Screen Shake) en saltos, impactos y rugidos.
- HUD en pantalla con velocímetro en km/h, estado del dinosaurio y monitor de conexión de Arduino.

## 🧪 Físicas del Proyecto

El proyecto tiene seis físicas. Cada una usa la fórmula que está en el script.

### 1. Choque con el árbol (`TreeImpactResponse`)

El árbol no rebota. Se cancela solo la velocidad que entra en el tronco:

$$
v_n = \vec{v}\cdot\hat{n}, \qquad \vec{J} = -m\,v_n\,\hat{n}, \qquad \vec{v}' = \vec{v} - v_n\,\hat{n}
$$

$v_n < 0$ es el dinosaurio empujando. $\vec{J}$ es el impulso (acción-reacción): el dinosaurio se frena en esa dirección y el árbol recibe el mismo impulso, al revés, y se mece. Al tercer golpe cae como una vara con pivote en la raíz:

$$
\tau = mg\frac{L}{2}\sin\theta, \quad I = \frac{mL^2}{3} \quad\Rightarrow\quad \alpha = \frac{3g}{2L}\sin\theta
$$

Se detiene a 82°.

### 2. Vuelo del pájaro (`AerodinamicaPajaro`)

$$
L = \tfrac12\rho v^2 S\,C_L, \quad D = \tfrac12\rho v^2 S\,C_D, \quad W = mg
$$

$\rho = 1{,}225$, $S = 0{,}05\,\mathrm{m}^2$, $C_L = 1{,}05$, $C_D = 0{,}4$, empuje $T = 0{,}42\,\mathrm{N}$, masa $0{,}08\,\mathrm{kg}$. El empuje va en el rumbo y oscila con el aleteo (5 Hz). Al escalar el pájaro, $S \propto s^2$ y la masa y el empuje $\propto s^3$.

### 3. Huida (`HuidaPajaro`)

Si el dinosaurio está a menos de 14 m, el rumbo es alejarse de él, suavizado con:

$$
\vec{d}(t) = \mathrm{Slerp}\big(\vec{d},\,\widehat{\vec{p}_\text{pájaro}-\vec{p}_\text{dino}},\, 1-e^{-4{,}5\,\Delta t}\big)
$$

El empuje se multiplica por 1,85, así el dinosaurio todavía lo alcanza. Lo atrapa a 2,4 m en horizontal y menos de 3,2 m de altura.

### 4. Orientación (`OrientacionPajaro`)

El cuerpo mira hacia la velocidad. El cabeceo sale de $v_y$. El banqueo es proporcional al giro, limitado a $\pm 38^\circ$:

$$
q(t) = \mathrm{Slerp}\big(q,\; \mathrm{LookRotation}(\vec{v})\cdot R_\text{banqueo},\; 1-e^{-8\,\Delta t}\big)
$$

### 5. Salto (`DinoVehicleController`)

En el aire la gravedad efectiva es $g = |g_\text{Unity}|\times 2{,}5$. Para una altura $h = 2\,\mathrm{m}$:

$$
v = \sqrt{2gh}
$$

Si suelta el borde, todavía puede saltar 0,15 s (coyote). Si pulsa **F** un poco antes de tocar suelo, el salto se guarda 0,15 s (buffer).

### 6. Pelota (`RebotePelota`, la Beach Ball)

$v_n$ es la velocidad de acercamiento a la superficie:

$$
v_n' = -e\,v_n, \quad v_t' = v_t\,(1-\mu), \quad e = 0{,}72,\; \mu = 0{,}22
$$

La energía del golpe queda en $e^2$ (cerca de la mitad de la altura cada bote). Si la salida baja de 0,7 m/s, deja de rebotar; si la rapidez total baja de 0,35 m/s, se queda quieta.

## 🎮 Controles de Prueba (Teclado)

| Tecla | Acción | Comportamiento |
|---|---|---|
| W / S | Acelerador / Reversa | Control proporcional de velocidad con zancadas dinámicas. |
| A / D | Dirección (Volante) | Giro con arco vehicular e inclinación lateral del cuerpo. |
| Espacio | Freno | Detiene la inercia con fuerza de fricción. |
| F | Salto / Pisotón | Impulso vertical y sacudida de cámara al caer. |
| E o Click Izq | Rugido Bestia (Turbo) | Emite rugido real, sacude la cámara y activa turbo (1.6x) por 3.5 segundos. |
| Left Shift | Sprint continuo | Aceleración sostenida. |
| R | Reiniciar posición | Reubica el carnotauro en la pista si vuelca. |

## 🔌 Conexión de Hardware (Arduino)

El sketch listo para cargar en la placa se encuentra en: `Assets/Dino/ArduinoSketch/DinoController.ino`

**Pinout recomendado:**
- Pin A0: Potenciómetro de Dirección (Volante / Manillar)
- Pin A1: Potenciómetro de Velocidad (Palanca / Pedal)
- Pin 2: Pulsador tipo Arcade para Salto / Pisotón (`INPUT_PULLUP`)
- Pin 3: Pulsador / Switch para Rugido / Turbo (`INPUT_PULLUP`)
- Baudrate: 115200 baudios

## 📁 Estructura del Proyecto

```
Assets/
├── Dino/
│   ├── ArduinoSketch/
│   │   └── DinoController.ino       # Código C++ para Arduino (Kjbarreto)
│   ├── Audio/
│   │   └── Rugido.wav               # Sonido de rugido de alta calidad
│   ├── Editor/
│   │   ├── BuildTestTrackScene.cs   # Generador de escena y build ejecutable
│   │   ├── DWSetup.cs               # Configuración de importador FBX y materiales
│   │   └── TakeScreenshot.cs        # Utilidad de captura de pantalla
│   ├── Materials/
│   │   ├── M_DW_Carnotauro_Piel.mat
│   │   ├── M_DW_Carnotauro_Ojos.mat
│   │   ├── M_DW_Silla_Cuero.mat
│   │   └── M_DW_Hierro_Cadenas.mat
│   ├── Model/
│   │   ├── DW_Carnotauro_UE5.fbx    # Modelo 3D con montura y rig
│   │   └── DW_Carnotaurus_Vehicle.controller
│   ├── Scenes/
│   │   └── DinoTestTrack.unity      # Pista de pruebas con rampas y obstáculos
│   ├── Scripts/
│   │   ├── ArduinoSerialConnector.cs# Comunicación Serial multihilo (Kjbarreto)
│   │   ├── DinoAnimationDriver.cs   # Sincronización de animaciones con física (davrosas)
│   │   ├── DinoFollowCamera.cs      # Cámara dinámica en 3ra persona con shake (jjplazas)
│   │   ├── DinoHUD.cs               # Interfaz gráfica y velocímetro (jjplazas)
│   │   ├── DinoInputManager.cs      # Abstracción de entradas Teclado / Arduino (jjplazas)
│   │   └── DinoVehicleController.cs # Controlador vehicular con Rigidbody (jjplazas)
│   └── Textures/
│       ├── DW_Carnotauro_Piel_NegroRojo.png
│       ├── DW_Carnotauro_Normales.png
│       └── DW_Carnotauro_Ojos.png
└── Plugins/
    └── System.IO.Ports.dll          # Soporte de puerto Serial para Unity (Kjbarreto)
```
