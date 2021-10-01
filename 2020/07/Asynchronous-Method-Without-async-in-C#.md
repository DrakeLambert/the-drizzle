# Asynchronous Method Without `async` in C\#

I recently changed a controller action's method signature like so:

```diff
  [HttpGet("{id}")]
- public Task async GetRecordAsync(int id) => await _recordRepository.GetAsync(id);
+ public Task GetRecordAsync(int id) => _recordRepository.GetAsync(id);
```

During code review, my team members quickly spotted this odd change. They questioned the removal of the `async` and `await` keywords.

## My Initial Response

Great catch! First off, I do agree that this looks questionable. Even after the explanation I give below, it may be beneficial to add the `async`/`await` keywords back just so this doesn't catch people off guard.

In short, we can safely omit `async`/`await` since the calling method, `GetRecordAsync`, is ultimately returning the same thing as what it's calling, `_recordRepository.GetAsync`. Instead of our method awaiting the completion of the underlying task and immediately returning the result, it just hands the task to the caller, basically saying, "Here, you await this instead of me".

The asynchronous nature of / ability to `await` a method depends on it's return type being `Task` or `Task<T>` (actually [the compiler uses duck typing](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async#return-types) to know which methods are awaitable, but I've simplified). You can `await` a `Task` because it exposes a [callback API](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.taskawaiter.oncompleted?view=netcore-3.1) that allows the framework to tell it what code to execute after whatever asynchronous operation finishes.

Adding the `async` keyword to a method declaration does something else that's very much under the covers. Asynchronous methods operate in repeating chunks of:

1. do some synchronous work (like data transformation)
2. `await` some I/O work

When the compiler sees an `async` method, it creates a new state machine class that has a state for each chunk of code to be executed. Each state runs the synchronous work it needs, starts some asynchronous I/O, then returns, allowing the thread to go execute other code somewhere else (like responding to another HTTP request). Eventually, the framework will come back around to the state machine (can be triggered by a few reasons) to see if the previous asynchronous operation has finished. If so it will process the next chunk.

Removing the `async`/`await` keywords prevents the generation of the state machine. This slightly decreases overhead of running the method. TBH, I wouldn't be surprised if the compiler is smart enough to optimize around this whole thing anyway.

## Pitfalls of Omitting `async`/`await`

After getting feedback on this post, I learned that you can run into several hard to understand bugs if the keywords are omitted by default.

Check out the [runnable demo code here](https://github.com/DrakeLambert/the-drizzle/tree/master/2020/07/OmittingAsyncDemo)!

### Missing Methods In the Stack Trace

Consider the following chain of methods:

```csharp
async Task Top()
{
    await Middle();
}

Task Middle() // keywords omitted here
{
    return Bottom();
}

async Task Bottom()
{
    await Task.Delay(10);
    throw new Exception("Bottom Exception");
}
```

When calling `Top()`, the resulting stack trace won't include the call to `Middle()`:

```text
System.Exception: Bottom Exception
   at OmittingAsyncDemo.WithoutKeywords.Bottom()
   at OmittingAsyncDemo.WithoutKeywords.Top()
   at OmittingAsyncDemo.Program.Main(String[] args)
```

### Cancellation of Tasks Dependent Upon `using`

The bug in the below code would leave me scratching my head. Calling `UsingStatementWithoutKeywords()` results in a `TaskCanceledException`.

```csharp
Task<string> UsingStatementWithoutKeywords()
{
    using var client = new HttpClient();
    return client.GetStringAsync("https://1.1.1.1");
}
```

This happens because the `HttpClient` is disposed before the request made ever completes, thus canceling any ongoing requests.

## Conclusion

Check out more subtle pitfalls of omitting `async`/`await` in [this article by Stephen Cleary](https://blog.stephencleary.com/2016/12/eliding-async-await.html).

Equipped with this new information, I would **recommend against removing the keywords by default.** As they say, premature optimization is the root of all evil. 😅 That's a little harsh, but I'm sure you catch my drift. I would consider removing the keywords if these conditions are met:

- The async method is merely a passthrough
- The code path has been shown to be very hot

If you find this post useful, and wish to support it, you can below!

<a href="https://www.buymeacoffee.com/drakelambert" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-blue.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>

Resources:

- [Async State Machine](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.asyncstatemachineattribute?view=netcore-3.1#remarks)
- [MS Docs: Async State Machine Remarks](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async#return-types)
- [`TaskAwaiter.OnCompleted()` Callback API](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.taskawaiter.oncompleted?view=netcore-3.1)
- [Async Await and the Generated StateMachine](https://www.codeproject.com/Articles/535635/Async-Await-and-the-Generated-StateMachine)
- [Runnable Demo Code Demonstrating Pitfalls](https://github.com/DrakeLambert/the-drizzle/tree/master/2020/07/OmittingAsyncDemo)
- [Eliding Async and Await - Stephen Cleary](https://blog.stephencleary.com/2016/12/eliding-async-await.html)