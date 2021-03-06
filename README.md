# Приёмник сообщений (T2.CLS.LoggerExtensions)

## Введение

**T2 Журнализация** - комплексное решения для сбора, хранения и анализа логов. Система гарантирует целостность логов, быстрый поиск, сжатый размер данных.

**Приёмник сообщений (target)** - компонент интегрируемый в логируемое приложение для сбора логов и отправки в транспорт логов. Поддерживается фреймворк NLog.

Для дополнительной информации и для ознакомления с **T2 Журнализация** можно обратиться к порталу документации:
https://galaktikasoftofficial.github.io/T2.CLS.Docs/

## Описание проекта

Решение состоит из проектов:
* **T2.CLS.LoggerExtensions.Core** - проект общей части, в которой реализован лог-буфер (файловый), обработка и отправка сообщение в транспорт.
* **T2.CLS.LoggerExtensions.NLog** - проект таргета для фреймворка NLog. 
* **T2.CLS.LoggerExtensions.Serilog** - проект таргета для фреймворка Serilog. (Находится в разработке). 

## Сборка проекта

### Сборка проекта на GitHub. 
 
Сборка происходит автоматически при отправке изменений в origin ветку (поддерживаются ветки master и develop). По результатам сборки будет создан релиз. Посмотреть релизы можно по адресу https://github.com/GalaktikasoftOfficial/T2.CLS.LoggerExtensions/releases. 

Созданные nuget пакеты привязываются к релизу и публикуются на внутренней nuget галерее: https://nuget.pkg.github.com/GalaktikasoftOfficial/index.json

### Локальная сборка проекта

Для выполнения сборки необходимо выполнить клон проекта.

Далее выполнить команды:

``` 
dotnet restore T2.CLS.LoggerExtensions.sln
dotnet build T2.CLS.LoggerExtensions.sln
```

## Участие в разработке

Разработку требуется вести в отдельной ветке для каждой задачи, сделанаю на основе ветки develop. Все изменения сливаются в develop ветку через pull request.

