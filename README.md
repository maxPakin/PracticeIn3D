# Разработка 3D-UI для системы безопасности на UWP
Здесь будут описаны основные требования, процесс их достижения и планы на будующую разработку.
## Техническое задание:
* **Входные данные** - пока не известно, откуда брать данные о состоянии датчиков и камер
* **3D-графика** - для отображения требуется создать дружелюбный интерфейс, чтобы пользователю было наглядно показано, где сработала сигнализация
* **Кроссплатформенность** - основной целью является *Raspberry PI*, но возможны и друге варианты
* **Пока всё...**
## Процесс разработки:
* **07.02.2020** - оговорено примерное техническое задание, создан проект на базе *UWP*
* **08.02.2020** - выбран пакет для отображения 3D-графики, а именно - *HelixToolkit для UWP (v 2.10.0)*. Создана первая сцена со светом, движением камеры и кубиком в центре экрана. К примеру, есть несколько видов света:  
**AmbientLight3D** - обтекающий свет (по-видимому, самый натуральный)  
**SpotLight3D** - направленный (центральные лучи)  
**DirectionalLight3D** - направленный (параллельные лучи)  
**PointLight3D** - точечный свет (как очень маленькое солнце)  
Ещё больше есть видов камер. Но скорей всего я буду использовать только *PerspectiveCamera* 
* **10.02.2020** - создан репозиторий на *GitHub*
## Планы:
* **11.02.2020** - найти способ загрузки .DXF-файлов
* **12.02.2020** - проверить возможность деплоя на *Raspberry PI*
