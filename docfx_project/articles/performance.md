# Performance

**Tracing comes at a cost of performance.**

This compoment writes the output asynchron in a background thread to not slow down the application to much. But capturing the objects for output is performed in the application thread.
So tracing large amounts of data will slow down your application. But you get all the information you need to understand what is going on.
Often this will make analysis easier of it able to show the order of events even in a multithread szenario.


**Important**: If you omit to close the trace there are chances that the last trace entries are not written, since the background thread is simply killed.

