# Инструкция пользователя

## Введение
DurableFluentdTarget - NLog target для гарантированной отправки лог сообщений во Fluentd. Гарантия доставки обеспечивается использованием файлового буффера и повторной отправкой сообщений, в случае недоступности сервиса Fluentd.

## Синтаксис конфигурации

    <target name="fluentd" 
            xsi:type="DurableFluentd"
            requestUri="http://by01-vm45:9880/T2.Cls.LogViewer.LoggerExtension"
            bufferPath="LogBuffer"
            memoryBufferLimit="256"
            fileBufferLimit="16384"
            workerCount="1"
            flushTimeout="5000"
            fluentBufferLimit="512">

      <attributes>
        <attribute name="Attribute" layout="Layout" />
      </attributes>

    </target>

## Параметры
- **name** - наименование target'а.
- **requestUri** - адрес назначения логов во Fluentd. Для использования круговой (round robin) баллансировки нагрузки дополнительные адреса указываются через запятую.
- **bufferPath** - путь к директории файлового буфера. Путь может задаваться как абсолютный, либо относительный (относительно AppContext.BaseDirectory)
- **memoryBufferLimit** - объем буфера памяти (колличество лог сообщений).
- **fileBufferLimit** - объем файлового буфера (колличество лог сообщений).
- **workerCount** - колличество потоков отправки сообщений.
- **flushTimeout** - временной интервал (мсек) записи буфера памяти в файловый буфер.
- **fluentBufferLimit** - объем буфера отправки сообщений во Fluentd (колличество лог сообщений).
- **attributes** - дополнительные атрибуты лог сообщений.

## Пример конфигурации

    <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" 
          xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    
      <!-- Подключение расширения LoggerExtensions -->
      <extensions>
        <add assembly="T2.CLS.LoggerExtensions.NLog"/>
      </extensions>
    
      <!-- Конфигурация Fluentd -->
      <targets>
        <target name="fluentd" 
                xsi:type="DurableFluentd"
                requestUri="http://by01-vm45:9880/T2.Cls.LogViewer.LoggerExtension"
                memoryBufferLimit="256"
                fileBufferLimit="16384"
                workerCount="1"
                bufferPath="LogBuffer"
                flushTimeout="5000"
                fluentBufferLimit="512">
    
          <attributes>
            <attribute name="SystemTag" layout="LoggerExtensions App" />
          </attributes>
    
        </target>
      </targets>
    
      <!-- Правила логирования -->
      <rules>
        <logger name="*" minlevel="Info" writeTo="fluentd" />
      </rules>
    
    </nlog>

## Конфигурация из кода

    var config = new LoggingConfiguration();
    var target = new DurableFluentdTarget
    {
      Name = "fluentd",
      RequestUri = new List<string>
      {
          "http://by01-vm45:9880/T2.Cls.LogViewer.LoggerExtension"
      },
      MemoryBufferLimit = 256,
      FileBufferLimit = 16384,
      WorkerCount = 1,
      BufferPath = "LogBuffer",
      FlushTimeout = 5000,
      FluentBufferLimit = 512,
      Attributes =
      {
          new FluentTargetAttribute{ Name = "Attribute", Layout = new SimpleLayout { Text  = "Value"}}
      }
    };
    
    config.AddTarget(target);
    config.AddRuleForAllLevels(target);
    
    LogManager.Configuration = config;