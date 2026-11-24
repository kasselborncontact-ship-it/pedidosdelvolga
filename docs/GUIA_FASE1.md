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

1. Bajá este repositorio: botón verde **Code → Download ZIP** en GitHub (o clonalo con GitHub Desktop).
   Asegurate de bajar la rama `claude/la-tranquera-farm-game-xsq2ei` (o la rama principal, si ya se unió).
2. Descomprimí el ZIP. Adentro hay una carpeta `Assets/LaTranquera`.
3. Copiá la carpeta **`LaTranquera`** completa dentro de la carpeta **`Assets`** de tu proyecto
   (queda `TuProyecto/Assets/LaTranquera/...`).
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
2. Tarda un poco: arma la ruta, el almacén, la huerta, el pozo, el galpón, la casa, los alambrados,
   los árboles, las sierras, el pasto, los sistemas y el jugador.
3. Al terminar queda guardada en `Assets/LaTranquera/Scenes/Fase1.unity` y agregada a la lista de escenas del juego.

> Podés volver a construirla cuando quieras (por ejemplo, si rompiste algo). Se reemplaza entera.

## Paso 6 · ¡A jugar! (la prueba de la Fase 1)

Apretá el botón **▶ Play** arriba en el centro. Hacé clic en la ventana Game para que tome el mouse.
**Esc** abre la pausa y libera el mouse.

Este recorrido es exactamente la condición de "Fase 1 terminada":

1. **Herramientas.** Caminá al **galpón** (el de chapa, a la derecha del almacén). Mirá la **azada** (parada en
   el rincón del fondo) y apretá **E**: la tenés en la mano.
2. **Carpir.** Andá a la **huerta** (a la izquierda, entrando por el lado este del alambrado). Mirá un cantero y hacé
   **clic izquierdo 4 veces** hasta que diga 100 %. La tierra cambia a surcos.
3. Volvé al galpón, mirá el gancho de la azada y apretá **E** para dejarla en su lugar (o soltala con **G** donde quieras).
4. **Sembrar.** En la mesa del galpón hay 3 sobres de semillas. Agarrá el de **tomate** (E), mirá el cantero carpido y
   **clic** en cada lugar donde quieras una planta (son 6 lugares por cantero). Aparece un brote.
5. **Regar.** Dejá el sobre (G) y agarrá la **regadera** (repisa del galpón). Andá al **pozo** (afuera de la huerta,
   del lado este) y hacé **clic** mirando el pozo: baja el balde y la regadera se llena (10 L).
   Mirá el cantero y **clic** dos veces (cada pasada usa 2,5 L y sube la humedad 50 %).
6. **Esperar.** El tomate necesita **96 horas regadas** (4 días de juego). La humedad dura ~24 h: **regá todos los días**.
   Si se seca más de 12 h se marchita; más de 48 h, se muere.
   - Para pasar la noche: entrá a la **casa** (atrás del almacén) y apretá **E** en la cama (desde las 19:00).
     Se guarda la partida.
   - 🔧 Para probar rápido en el editor: **F8** adelanta 6 horas.
7. **Cosechar.** Cuando la planta tiene tomates rojos dice "lista para cosechar". Agarrá un **cajón de madera**
   (hay 3 en la entrada de la huerta), mirá la planta y **clic**: los 4 tomates van al cajón y se ven adentro.
   Un cajón lleva hasta 24 tomates (u 8 lechugas). Cada cajón lleva un solo producto.
8. **Al almacén.** Caminá con el cajón hasta el almacén. Mirá un lugar vacío de una **estantería** y **clic**:
   pasa un tomate por clic. Cada lugar muestra una etiqueta con nombre y precio.
9. **Precio.** Opción A: la **etiquetadora** (en la repisa al lado de la computadora): con ella en la mano mirá el
   tomate y usá la **rueda del mouse** (+/- $100). Opción B: la **computadora** (E) → pestaña **Precios**.
   El sugerido del tomate es **$600**. Si lo ponés más de 35 % arriba, nadie lo compra y se quejan.
10. **Abrir.** El cartel cuelga en la vidriera que está a la izquierda de la puerta (mirando el almacén desde afuera).
    Entrá, miralo desde adentro y apretá **E**: pasa a **ABIERTO** (se lee desde la ruta).
11. **Cobrar.** Pasate atrás del mostrador (por el lado de la heladera). Cuando un cliente apoya sus cosas:
    1. Mirá cada producto y **E**: pasa por el lector (bip) y se suma en la pantalla.
    2. El cliente dice cómo paga.
    3. **Efectivo:** sus billetes quedan sobre el mostrador. **E en el frente del cajón** para abrirlo, después
       **E en los billetes del cliente** para guardarlos. La pantalla muestra el **VUELTO**.
       Mirá cada compartimento del cajón ($100, $200, $500, $1.000...) y **E** para pasar un billete a la
       **bandeja del vuelto**. Si te equivocaste, **E en la bandeja** devuelve el último.
       Cuando la bandeja tenga justo el vuelto, **E en el botón rojo ENTREGAR VUELTO**.
       Si está mal, el cliente reclama y espera que lo corrijas.
    4. **Tarjeta:** mirá las teclas del **posnet** y apretá **E** en cada número para tipear el total, después **OK**.
       Si el monto no es el correcto, sale RECHAZADA y el cliente te avisa.

✅ Si llegaste hasta acá, la Fase 1 funciona.

### Otras cosas para probar
- **Pedidos al proveedor:** computadora → **Proveedor**. Tocá **+** en los productos (cada uno tiene su foto),
  mirá el carrito y el total, **Confirmar pedido**. ~1 h 30 min de juego después llega una F-100 celeste y deja las
  cajas en la puerta del galpón. Llevalas adentro, **F** para abrir cada caja y reponé como con los cajones.
  Las cajas vacías van al **tacho de basura** (al lado de la puerta del almacén, E con la caja en la mano).
- **Lácteos** (leche, queso, manteca) solo entran en la **heladera**.
- **Mejoras:** computadora → **Mejoras** → comprar canteros extra ($20.000 c/u).
- **Caja y banco:** ver ventas del día y depositar la caja (al dormir se deposita sola, dejando cambio chico).
- Arrancás con **$40.000 en el banco** y **$19.000 de cambio** en la caja, más una caja de yerba, una de fideos y una
  de leche en el galpón para tener algo que vender el primer día. La luz cuesta $1.800/día + $900 por heladera.

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
| `Interactivo/Huerta/Cantero_1..12` | **GardenBed** (adentro, 6 `Lugar_n` con **Plant**). Del 7 al 12 empiezan bloqueados. |
| `Interactivo/Pozo` | **WaterSource** |
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
| Precio de los canteros extra | cada `Cantero_n` → GardenBed → Unlock Price |
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
