# Guía paso a paso – Fase 1

Esta guía está pensada para alguien que **nunca usó Unity**. Seguila en orden.

---

## Paso 1 · Instalar Unity

1. Bajá e instalá **Unity Hub**: https://unity.com/download
2. Abrí Unity Hub → iniciá sesión (la cuenta es gratis) → pestaña **Installs** → **Install Editor**.
3. Elegí **Unity 6 (LTS)** (la versión 6000.0.x más nueva que aparezca).
4. En "Modules" tildá:
   - **Microsoft Visual Studio Community** (si no lo tenés)
   - **Windows Build Support (IL2CPP)**
5. Esperá a que termine (son varios GB).

## Paso 2 · Crear el proyecto

1. En Unity Hub → **Projects** → **New project**.
2. Elegí la plantilla **Universal 3D** (es la de URP). ⚠️ No elijas "3D (Built-In)".
3. Nombre: `LaTranquera`. Ubicación: una carpeta tuya (por ejemplo `Documentos/Juegos`).
4. **Create project**. Se abre el editor de Unity (la primera vez tarda).

## Paso 3 · Copiar el juego dentro del proyecto

1. Bajá el código de la rama correcta con este enlace directo (tenés que estar con tu cuenta de GitHub abierta):
   https://github.com/kasselborncontact-ship-it/pedidosdelvolga/archive/refs/heads/claude/la-tranquera-farm-game-xsq2ei.zip
   > ⚠️ Si bajás el ZIP desde la página principal sin cambiar la rama, te baja la rama `main`, que **solo tiene el README**.
   > Para hacerlo a mano: en GitHub, arriba a la izquierda, el botón que dice **main** → elegí
   > `claude/la-tranquera-farm-game-xsq2ei` → después **Code → Download ZIP**.
2. Descomprimí el ZIP (clic derecho → **Extraer todo**). Queda una carpeta llamada
   `pedidosdelvolga-claude-la-tranquera-farm-game-xsq2ei`. Entrá: adentro está **`Assets`**, y adentro de esa, **`LaTranquera`**.
3. Abrí también la carpeta de tu proyecto de Unity (la que creaste en el Paso 2, por ejemplo `Documentos/Juegos/LaTranquera`).
   Adentro tiene una carpeta **`Assets`**. Copiá la carpeta **`LaTranquera`** del ZIP y pegala dentro de ese `Assets`.
   Tiene que quedar: `LaTranquera (tu proyecto)/Assets/LaTranquera/Scripts/...`
   > Tip: en Unity, clic derecho sobre la carpeta **Assets** (ventana Project) → **Show in Explorer** te abre la carpeta justa.
4. Volvé a Unity: va a importar los archivos solo. Mirá la ventana **Console** (abajo):
   **no tiene que haber errores rojos**. (Si hay advertencias amarillas, no pasa nada.)

## Paso 4 · Revisar el sistema de teclas (Input System)

El juego usa el sistema de teclas nuevo de Unity ("Input System"), que ya viene con la plantilla Universal 3D.

1. Menú **Edit → Project Settings → Player → Other Settings**.
2. Buscá **Active Input Handling** y poné **Input System Package (New)** o **Both**.
3. Si Unity te pide reiniciar, aceptá.

Si en la Console aparece un error que menciona `UnityEngine.InputSystem`:
**Window → Package Manager → Unity Registry → Input System → Install**.

## Paso 5 · Construir la escena (un clic)

1. En la barra de menús de arriba apareció **La Tranquera**. Hacé clic en
   **La Tranquera → Construir escena Fase 1** y aceptá.
2. Tarda un poco: arma la ruta, la casa de campo con el almacén, el depósito, los terrenos, el aljibe, el tambo, los alambrados,
   los árboles, las sierras, el pasto, los sistemas y el jugador.
3. Al terminar queda guardada en `Assets/LaTranquera/Scenes/Fase1.unity` y agregada a la lista de escenas del juego.

> Podés volver a construirla cuando quieras (por ejemplo, si rompiste algo). Se reemplaza entera.

## Paso 6 · ¡A jugar! (la prueba de la Fase 1)

Apretá el botón **▶ Play** arriba en el centro. Hacé clic en la ventana Game para que tome el mouse.
**Esc** abre la pausa y libera el mouse.

Arrancás en el estacionamiento, frente a la **casa de campo**. Adelante, bajo la galería, está el **almacén**.
A la derecha de la casa está el **depósito** (con portón al camino de tierra) y a la izquierda, cruzando el
caminito, los **terrenos para sembrar**. Atrás de la casa está el patio con el **aljibe**.

Este recorrido es exactamente la condición de "Fase 1 terminada":

1. **Herramientas.** Entrá al almacén, pasá por la puerta del fondo al **comedor** y de ahí a la derecha al **depósito**.
   En la pared del fondo están la **azada**, la **regadera** y el **balde**. Mirá la azada y apretá **E**.
2. **Carpir donde quieras.** Andá a los **terrenos** (salís por la galería hacia la izquierda y entrás por la
   abertura del alambrado). El **Lote 1** es tuyo. Mirá cualquier lugar de la tierra y hacé **clic izquierdo 2 veces**:
   queda un surco carpido. Hacé todos los que quieras (tienen que estar un poco separados).
3. Volvé y dejá la azada en su gancho (**E**) o soltala con **G** donde quieras.
4. **Sembrar.** En la mesa del depósito hay 3 sobres de semillas. Agarrá el de **tomate** (E), mirá un surco y **clic**.
   Los cultivos grandes (zapallo, sandía) piden más lugar libre alrededor: el juego te avisa.
5. **Regar.** Agarrá la **regadera**, andá al **aljibe** (patio de atrás) y **clic** mirándolo: baja el balde y se llena.
   Mirá tus plantas y **clic**: riega todas las que estén cerca del punto que mirás (1 L cada una).
6. **Esperar.** El tomate necesita **96 horas regadas** (4 días de juego). La humedad dura ~24 h: **regá todos los días**.
   Si se seca más de 12 h se marchita; más de 48 h, se muere.
   - Para pasar la noche: andá al **dormitorio** (a la izquierda del almacén, se entra por la cocina) y **E** en la cama
     (desde las 19:00). Se guarda la partida.
   - 🔧 Para probar rápido en el editor: **F8** adelanta 6 horas.
7. **Cosechar.** Cuando tiene tomates rojos dice "lista para cosechar". Agarrá un **cajón** (hay 3 en la entrada
   del Lote 1), mirá la planta y **clic**: los 4 tomates van al cajón.
8. **Al almacén.** Llevá el cajón al almacén, mirá un lugar de una **estantería** y **clic** (un tomate por clic).
9. **Precio.** Con la **etiquetadora** (repisa al lado de la computadora) usá la **rueda del mouse**, o en la
   **computadora** (E) → **Precios**. El sugerido del tomate es **$600**.
10. **Abrir.** El cartel cuelga en la vidriera a la izquierda de la puerta (mirando desde afuera). Miralo desde adentro
    y apretá **E**: pasa a **ABIERTO**.
11. **Cobrar.** Pasate atrás del mostrador (por el lado de la heladera). Cuando un cliente apoya sus cosas:
    1. Mirá cada producto y **E**: pasa por el lector (bip) y se suma en la pantalla.
    2. **Efectivo:** **E en el frente del cajón** para abrirlo, **E en los billetes del cliente** para guardarlos,
       y armá el **VUELTO** sacando billetes de cada compartimento (E). **E en la bandeja** devuelve el último.
       Cuando esté justo, **E en el botón rojo ENTREGAR VUELTO**.
    3. **Tarjeta:** tipeá el total en el **posnet** (E en cada tecla) y **OK**.

✅ Si llegaste hasta acá, la Fase 1 funciona.

### Otras cosas para probar
- **Pedidos al proveedor:** computadora → **Proveedor**. Tocá **+** en los productos (cada uno tiene su foto),
  mirá el carrito y el total, **Confirmar pedido**. ~1 h 30 min de juego después entra una F-100 celeste por la
  tranquera y el camino de tierra, y deja las cajas en la puerta del depósito. Llevalas adentro, **F** para abrir cada caja y reponé como con los cajones.
  Las cajas vacías van al **tacho de basura** (al lado de la puerta del almacén, E con la caja en la mano).
- **Lácteos** (leche, queso, manteca) solo entran en la **heladera**.
- **Mejoras:** computadora → **Mejoras** → comprar más terrenos (Lote 2 y 3) y semillas nuevas (papa, cebolla, maíz, zapallo, sandía, frutilla).
- **La casa:** en la cocina podés cebarte unos mates, prender la radio o atizar la cocina a leña; el velador del dormitorio se prende y se apaga; las puertas se abren y cierran con E.
- **El lechero:** todas las mañanas a las 7 pasa en su Rastrojero hasta el galpón del tambo (en la Fase 4 te va a comprar la leche).
- **Caja y banco:** ver ventas del día y depositar la caja (al dormir se deposita sola, dejando cambio chico).
- Arrancás con **$40.000 en el banco** y **$19.000 de cambio** en la caja, más una caja de yerba, una de fideos y una
  de leche en el depósito para tener algo que vender el primer día. La luz cuesta $1.800/día + $900 por heladera.

---

## Paso 7 · Dónde va cada script (qué armó el constructor)

No hace falta hacerlo a mano, pero así podés entender y cambiar la escena (ventana **Hierarchy**):

| Objeto en la escena | Componentes (scripts) |
|---|---|
| `Sol` | Light (Directional) + **DayNightCycle** |
| `_Sistemas` | **GameManager, TimeManager, Economy, StoreManager, DeliveryManager, CustomerSpawner, TrafficSpawner, HUD, ComputerUI, NewGameSetup** |
| `_Sistemas/RutaCamioneta` | puntos por donde entra y sale la camioneta |
| `CaminoClientes` | **StoreNav** + puntos por donde caminan los clientes |
| `EventSystem` | EventSystem + InputSystemUIInputModule (para los botones) |
| `Jugador` | CharacterController + **PlayerController** + **PlayerHands** |
| `Jugador/CamaraPivot/Main Camera` | Camera + **Interactor** (y adentro `Manos`, donde se ve lo que llevás) |
| `Interactivo/Almacen/Estanteria_1`, `_2` | **Shelf** |
| `Interactivo/Almacen/Heladera` | **Shelf** con *Is Fridge* tildado |
| `Interactivo/Almacen/Mostrador` | **CheckoutCounter** (y sus puntos: cliente, fila, lector, embolsado) |
| `.../Mostrador/PantallaCaja` | **RegisterScreen** |
| `.../Mostrador/CajonBilletes` | **CashDrawer** (adentro, un **BillSlot** por billete) |
| `.../Mostrador/BandejaVuelto` | **ChangeTray** |
| `.../Mostrador/BotonEntregar` | **GiveChangeButton** |
| `.../Mostrador/Posnet` | **Posnet** (adentro, un **PosnetKey** por tecla) |
| `Interactivo/Almacen/Computadora` | **ComputerTerminal** |
| `Interactivo/Almacen/CartelAbiertoCerrado` | **OpenSign** |
| `Interactivo/Almacen/TachoBasura` | **TrashBin** |
| `Interactivo/.../LugarAzada`, `LugarRegadera`, `LugarBalde`, `LugarEtiquetadora` | **ToolHook** |
| `Interactivo/Terrenos/Lote 1..3` | **LandPlot** (el Lote 1 es tuyo; los surcos que carpís se crean solos como **SoilPatch**) |
| `Interactivo/Aljibe` | **WaterSource** |
| `Interactivo/Casa/...` | **Door** (puertas), **Decor** (mate, radio, cocina a leña, velador), **SleepBed** (cama) |
| `_Sistemas` | también **MilkmanVisit** (el lechero) |
| `Interactivo/Cama` | **SleepBed** |
| `Interactivo/Tranquera` | **FarmGate** |
| `Pasto` | **GrassField** |
| `Mundo (estático)/Sierras de fondo` | **HillsBackdrop** |

Los objetos que se agarran (cajones, cajas, herramientas, semillas, productos) **no** están en la escena:
los crea `NewGameSetup` al empezar una partida nueva, o `SaveSystem` al cargar una partida.

## Paso 8 · Cambiar números del juego

| Quiero cambiar... | Dónde |
|---|---|
| Precios sugeridos, costos, tamaños de caja, cuántos entran en cajón/estante | `Scripts/Items/ProductDB.cs` (lista `ProductDB.All`) |
| Tiempo de crecimiento, cosecha por planta, semillas por sobre | `Scripts/Items/ProductDB.cs` (lista `CropDB.All`) |
| Velocidad del reloj | `_Sistemas` → TimeManager → **Minutos Por Segundo** (1 = un minuto de juego por segundo) |
| Plata inicial y cambio en la caja | `_Sistemas` → NewGameSetup |
| Cantidad y frecuencia de clientes | `_Sistemas` → CustomerSpawner |
| Factura de luz | `_Sistemas` → Economy |
| Precio de terrenos y semillas nuevas | `Scripts/Economy/Upgrades.cs` |
| Lugar que necesita cada cultivo | `Scripts/Items/ProductDB.cs` → `spacing` |
| Textos / traducción al inglés | `Scripts/Core/Loc.cs` |

> Tras cambiar valores, si querés empezar de cero: **La Tranquera → Borrar partida guardada**.

## Paso 9 · Reemplazar formas simples por modelos lindos

1. Bajá un pack (ver el README): por ejemplo **Kenney Food Kit**. Descomprimilo.
2. Arrastrá la carpeta del pack a `Assets/` (por ejemplo `Assets/Packs/KenneyFood`).
3. Creá la carpeta `Assets/LaTranquera/Resources/Productos/`.
4. Arrastrá el modelo del tomate a la escena, ajustá **Scale** para que mida unos 8 cm y que quede apoyado en y = 0,
   y arrastralo desde Hierarchy a `Resources/Productos/`. Renombralo **`tomate`**. Borralo de la escena.
5. Play: los tomates de las estanterías, cajones y del mostrador ya usan tu modelo.

Lo mismo para plantas (`Resources/Plantas/tomate_3`), clientes (`Resources/Clientes/...`) y vehículos
(`Resources/Vehiculos/F100`). La lista completa está en `Assets/LaTranquera/Resources/LEEME.txt`.

**Si un modelo se ve rosa:** es porque su material no es de URP. Seleccioná el material y
**Edit → Rendering → Materials → Convert Selected Built-in Materials to URP**.

**Texturas de Poly Haven** (ripio, tierra, asfalto): bajá la textura en 1K, arrastrala a Unity, abrí el material que
querés cambiar en `Assets/LaTranquera/Generated/Materials` (por ejemplo el del ripio) y poné la imagen en **Base Map**.

## Paso 10 · Hacer el .exe para Windows

1. **File → Build Profiles** (en Unity 6) → **Windows**.
2. Verificá que `Fase1` esté en la lista de escenas.
3. **Player Settings** → Company Name / Product Name ("La Tranquera").
4. **Build** → elegí una carpeta vacía. Adentro queda `La Tranquera.exe`.

---

## Problemas comunes

| Problema | Solución |
|---|---|
| Todo se ve **rosa** | El proyecto no es URP. Creá el proyecto con la plantilla **Universal 3D**. |
| No aparece el menú **La Tranquera** | Hay errores rojos en la Console que no dejan compilar. Leé el primero. Suele ser el Input System (Paso 4). |
| Las teclas no hacen nada | Hacé clic dentro de la ventana Game. Revisá *Active Input Handling* (Paso 4). |
| No llegan clientes | ¿El cartel dice ABIERTO? Llegan de 7:30 a 20:30 y tienen que caminar desde la ruta (~30 s). |
| Los clientes dicen "¿No te queda...?" | Ese producto no está en ninguna estantería. |
| "va en la heladera" | Los lácteos solo se ponen en la heladera. |
| No puedo dar el vuelto | Abrí el cajón (E en el frente), guardá primero los billetes del cliente, y armá el vuelto en la bandeja. |
| La planta no crece | Mirala: muestra la humedad. Si es 0 %, regá. Si dice marchita, regá rápido. |
| Quiero empezar de nuevo | Esc → **Nueva partida**, o menú **La Tranquera → Borrar partida guardada**. |
