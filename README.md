# PostAsyncTests

Simple testing of UWP and HttpClient.PostAsync()

*Notes*

The webserver is currently sleeping for 10 seconds before returning the response for POST requests. This is to allow cancelling etc. when running the PostAsync task. To change that to something longer modify line 41 in MainController.cs:

 if (loopCount >= 10)  <== seconds to wait.
 
The TaskOne/TaskTwo tests (Start button), run indefinitely. Something to point out is that the cancellation is only going to cancel the underlying task for PostAsync(), it's obviously not going to cancel the HTTP request on the server since it has no way to do that. It's also not guaranteed that HttpClient.PostAsync will immediately cancel since there is no guarantee with cancellation sources. I did prove the task that made the request does cancel though.

I may test this with Wireshark in loopback mode, this will confirm if/when/how HttpClient.PostAsync() reacts to the cancellation of the token.

When clicking the Start button, this just runs TaskOne and TaskTwo, both of which just sleep 1 second, post a message that they are running, rinse/repeat. This is just for testing task cancellation outside of the HttpClient code.

To test:

1. Make sure the UWP App is the startup project.
2. Build the remote web server project.
3. Go into the bin directory for the server project and manually launch RemoteWebServer.exe
4. Now build and deploy the UWP app which can be ran from the IDE or separately in the OS.
5. Click either Start Or PostAsync.
6. Click Cancel to inform the CancellationToken source that we wish to cancel.
7. Thread counts are listed in the log window and the bottom right but my GUI thread hack for updating doesn't update the textblock when using PostAsync() - that's OK, as it still gets logged in the log window. This is how we can prove the tasks were cancelled because we can see the thread count increment/decrement.

*Note: The server is configured to listen on port 5000 when not in the IDE - if you try to run the web server from the IDE, the UDE app will fail because the port will change to the IISExpress port which isn't 5000; and the UWP app is hardcoded to use 5000.*
