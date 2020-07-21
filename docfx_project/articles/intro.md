# Introduction

When it comes to trace output it is rather handy if it exits. Though creating it sometimes is a hassle.
Therefore this component makes it easy to create a trace output with all information needed. 
The output can be customized to your needs.

Unfortunately the `System.Diagnostic.Trace` does not give acces to the `TraceData()` method, which is more than handy to output whole objects to the trace.

## Basics
The trace always consists of a `TraceSource` which gets called to trace some data and a number of attached `TraceListener` instances. These create the output for the selected targets.

The `TraceSource` and `TraceListener` can be created by code or by the configuration file (i.e `app.config`).
The source should be stored in some location, where you can access it convieniently from your code. Most of the time this seems to be a public static property of some class.

Here is a simple example from code of the TestApp.
```
    static void TestFromCode()
        {
            var source = new TraceSource("TestFromCode", SourceLevels.All);
            var listener = new ObjectFileTraceListener { Filename = "trace.txt" };
            source.Listeners.Add(listener);

            source.TraceInformation("Hello trace!");

            source.Close();
        }
```

The first line creates a new `TraceSource`. Be sure to set the `SourceLevels.All`. Otherwise you will miss some output or all - even where the methods are called.

The next lines create an `ObjectFileTraceListener` and attaches it to the source.

The next line creates a trace entry.

The last line closes the trace an flushes all output to the file. 

The file `trace.txt` will contain the output of the following form.
```
[10.07.2020 13:40:50] <P=2168> <T=1> TestFromCode: Information[0] - Void TestFromCode() - Hello trace!
```

The line consists of
* the timestamp when the trace entry was created.
* the process id (`P=2168`)
* the thread id (`T=1`)
* the name of the `TraceSource`
* the kind of trace (`TraceEventType`) and an id, which in this case is automatically `0`.
* the method calling the trace source
* the message given

## Some objects
If objects have to be trace they can easily be added. Consider a data object like
```
    class SimpleData
    {
        public string Name { get; set; }
        public int Number { get; set; }
        [NotTraceable]
        public string Secret => "This should not appear in the trace.";

        public DateTime TimeStamp { get; set; } = DateTime.Now;

        [TraceConverter(typeof(TraceConverterValueType), "yyyy-MM-dd-HH-mm-ss-ffffff")]
        public DateTime Detail { get; set; } = DateTime.Now;
    }
```

and the code as
```
        static void TestDataFromCode()
        {
            var source = new TraceSource("TestDataFromCode", SourceLevels.All);
            var listener = new ObjectFileTraceListener { Filename = "trace.txt" };
            source.Listeners.Add(listener);

            var data = new SimpleData { Name = "Alice" };

            source.TraceData(TraceEventType.Error, 42, "some text", data);

            source.Close();
        }
```

Then the trace will look like this.
```
[10.07.2020 13:59:20] <P=15636> <T=1> TestDataFromCode: Error[42] - Void TestDataFromCode() - 2 object(s)
  [0] = 'some text'
  [1] = Toolbox.Trace.TestApp.SimpleData
      Name = 'Alice'
      Number = 0
      TimeStamp = 10.07.2020 13:59:20
      Detail = 2020-07-10-13-59-20-171750
```

The first line will contain the information as above. Followed by the output of the the objects. 
This first object (index `0`) is just the `string` and the second the instance of `SimpleData`.

Note: The property `Secret` is not traced due to the `NotTraceable` attribute and `Detail` has a special formatting attached to it though the `TraceConverter` attributes. 
These will be explained later. And there are more options that customize the output.

