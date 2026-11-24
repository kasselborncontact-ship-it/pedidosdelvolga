# La Tranquera (nombre provisorio)

Simulador en primera persona de una granja chica argentina con almacén de campo sobre la ruta.
Hecho en **Unity 6 + URP + C#**, salida para **Windows** y preparado para Steam / Epic más adelante.

> 👉 **Empezá por la guía paso a paso:** [`docs/GUIA_FASE1.md`](docs/GUIA_FASE1.md)

---

## 1. Plan de fases

| Fase | Qué agrega | Estado |
|---|---|---|
| **1. Huerta + almacén** | Jugador, manos (una cosa por vez), cajones, huerta con 6 canteros y 3 cultivos (lechuga, tomate, zanahoria), pozo con balde, galpón con herramientas, almacén de 6 × 8 m con 2 estanterías y 1 heladera, precios (computadora y etiquetadora), cartel ABIERTO/CERRADO, clientes con fila y paciencia, caja con lector, cajón de billetes, vuelto y posnet, pedidos al proveedor con catálogo con fotos y camioneta, día/noche, dormir, gastos de luz, guardado, canteros extra que se compran | ✅ **Hecha (este repo)** |
| 2. Animales | Corrales y gallinero (se compran), ternera/ternero/lechón/pollitos, comedero con bolsas de alimento, bebedero con balde, crecimiento diario visible, nidos y canasta de huevos, maples | Siguiente |
| 3. Carnicería | Faena, res colgada en la cámara, mesa de corte con cuchillo o sierra, fetas, envasadora al vacío con etiqueta, heladera | |
| 4. Tambo y lácteos | Ordeñe a mano u ordeñadora, tanque, mesa de envasado (botellas de vidrio, tachos de 10 L), envases retornables, el lechero en Rastrojero | |
| 5. Vehículos y reparto | Clientes que llegan en auto, repartidor empleado que carga la camioneta y vende en la ciudad, sueldos | |

Regla de oro del proyecto: **todo es físico**. No hay inventario mágico: lo que llevás es lo que tenés en las manos.

## 2. Qué tenés que instalar

1. **Unity Hub** – https://unity.com/download
2. **Unity 6 LTS** (6000.0.x o más nuevo) desde Unity Hub, con el módulo **Windows Build Support (IL2CPP)**.
3. **Visual Studio Community 2022** con la carga de trabajo "Desarrollo de juegos con Unity" (Unity Hub te lo ofrece al instalar Unity).
4. (Opcional) **Git** o **GitHub Desktop** para bajar este repositorio y guardar tus cambios.

Nada más: la Fase 1 funciona **sin descargar modelos**, porque todo está armado con formas simples hechas por código.
Cuando quieras que se vea más lindo, reemplazás cada cosa por un modelo (ver punto 4).

## 3. Cómo está organizado el código

Todo vive en `Assets/LaTranquera/`:

```
Scripts/
  Core/         GameInput (teclas), GameManager (pausa, dormir), NewGameSetup (partida nueva),
                Mats (materiales), Shapes (formas), Sfx (sonidos), Loc (traducciones)
  Player/       PlayerController (caminar, mirar), PlayerHands (lo que llevás)
  Interaction/  IInteractable, Interactor (el "punto" del centro: qué mirás y qué tecla usar)
  Items/        Carryable (base de lo que se agarra), Container/Crate/SupplierBox (cajones y cajas),
                ProductItem, ProductDB (TODOS los productos y cultivos), herramientas (Hoe, WateringCan,
                Bucket, SeedPacket, PriceLabeler), WorldFactory, ProductVisuals
  Farming/      GardenBed (cantero), Plant (planta con 4 etapas), PlantVisuals, WaterSource (pozo)
  Store/        StoreManager, Shelf (estantería/heladera), OpenSign, ToolHook, TrashBin, FarmGate
  Customers/    Customer, CustomerSpawner, CustomerVisuals, SpeechBubble, StoreNav
  Checkout/     CheckoutCounter (la venta), CounterItem, RegisterScreen, CashDrawer + BillSlot,
                ChangeTray, GiveChangeButton, CustomerCash, Posnet + PosnetKey, Cash (billetes)
  Supplier/     SupplierCatalog, DeliveryManager, DeliveryTruck, ComputerTerminal, ComputerUI
  Economy/      Economy (banco, gastos, estadísticas) y PriceBook (precios de venta)
  Time/         TimeManager (reloj), DayNightCycle (sol y luces), SleepBed (cama)
  Save/         SaveData, SaveSystem (JSON)
  UI/           HUD, UIKit, IconRenderer (fotos del catálogo)
  World/        GrassField (pasto con volumen), HillsBackdrop (sierras de fondo)
  Vehicles/     VehicleVisuals (F-100, Rastrojero, Falcon), TrafficSpawner, PassingCar
Editor/
  SceneBuilder  menú "La Tranquera > Construir escena Fase 1" (arma todo el mapa solo)
  WorldParts    paredes, alambrados, árboles, postes de luz, carteles
```

## 4. Assets recomendados (gratis o baratos)

| Para qué | Pack | Precio |
|---|---|---|
| Personajes (clientes, lechero) con animaciones | **Quaternius** – "Ultimate Modular Men / Women" o "Animated Characters" (quaternius.com) | Gratis (CC0) |
| Animales de granja (fase 2) | **Quaternius** – "Ultimate Animated Animals" / "Farm Animals" | Gratis (CC0) |
| Árboles, pasto, piedras | **Quaternius** – "Stylized Nature MegaKit" / "Ultimate Nature Pack" | Gratis (CC0) |
| Comida, verduras, envases | **Kenney** – "Food Kit" (kenney.nl) | Gratis (CC0) |
| Muebles, estanterías, heladera | **Kenney** – "Furniture Kit" | Gratis (CC0) |
| Autos (base para modificar) | **Kenney** – "Car Kit" | Gratis (CC0) |
| Todo junto con el mismo estilo | **Synty** – "POLYGON Farm" (Unity Asset Store) | Pago (~USD 30–60, suele estar en oferta) |
| Texturas de ripio, tierra, asfalto, chapa | **Poly Haven** (polyhaven.com) | Gratis (CC0) |

Cómo usarlos (sin tocar código): el juego busca modelos en carpetas `Resources` con nombres fijos.
Si encuentra uno, lo usa; si no, dibuja la forma simple. Detalles en
[`Assets/LaTranquera/Resources/LEEME.txt`](Assets/LaTranquera/Resources/LEEME.txt) y en la guía.

> ⚠️ Para publicar en Steam: los nombres y logos de marcas reales (Ford, Falcon, F-100, Rastrojero) pueden
> necesitar licencia. En el juego conviene usar nombres genéricos ("la chata", "el sedán") y no poner logos.
> Lo mismo con los billetes: usá diseños propios con los valores reales, no copias de los billetes.

## 5. Controles

| Tecla | Acción |
|---|---|
| WASD / Shift / Espacio | Caminar / correr / saltar |
| Mouse | Mirar |
| **E** | Agarrar lo que mirás, apretar botones, usar cosas (cartel, caja, posnet, cama, computadora) |
| **G** | Soltar o apoyar lo que tenés en las manos (en el piso o en una mesa) |
| **Clic izquierdo** | Usar lo que tenés en la mano sobre lo que mirás (carpir, sembrar, regar, cosechar, reponer) |
| **F** | Abrir una caja del proveedor |
| Rueda del mouse | Cambiar el precio (con la etiquetadora en la mano) |
| F5 | Guardar · **Esc** pausa · F8 adelantar 6 h (solo para probar, en el editor) |

## 6. Steam y Epic (más adelante)

Cuando la Fase 1 esté pulida: instalar **Steamworks.NET** (paquete gratuito), crear la app en Steamworks
(USD 100 por juego), y conectar logros y Steam Cloud al `SaveSystem` (el archivo `partida.json`).
El código ya separa el guardado en un solo lugar para que eso sea fácil.
