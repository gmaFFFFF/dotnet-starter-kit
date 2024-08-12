using System.ComponentModel;
using System.Reactive.Linq;

namespace System.IO;

public static class FileSystemWatcherРасширения {
    public static IObservable<FileSystemEventArgs> ToObservable(this ИFileSystemWatcherОбёртка @this) {
        var fswObservableWrapper = Observable.Defer(() => {
                @this.EnableRaisingEvents = true;
                return Observable.Return(@this);
            })
            // Каталог не существует
            .Catch((ArgumentException exc) => Observable.Throw<ИFileSystemWatcherОбёртка>(exc))
            // Неизвестное исключение
            .Catch((Exception exc) => Observable.Throw<ИFileSystemWatcherОбёртка>(exc))
            .Publish()
            .RefCount(2);

        var событияФС = fswObservableWrapper
            .SelectMany(fsw =>
                Observable.Merge(Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                        h => fsw.Created += h, h => fsw.Created -= h),
                    Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                        h => fsw.Changed += h, h => fsw.Changed -= h),
                    Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                        h => fsw.Deleted += h, h => fsw.Deleted -= h),
                    Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(
                        h => fsw.Renamed += h, h => fsw.Renamed -= h))
            )
            .Select(evt => evt.EventArgs);

        var ошибкиНаблюдения = fswObservableWrapper
            .SelectMany(fsw =>
                Observable.FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
                    h => fsw.Error += h, h => fsw.Error -= h))
            .SelectMany(evt =>
                evt.EventArgs.GetException() switch {
                    // Произошло слишком много файловых событий и внутренний буфер FileSystemWatcher не может с ними справится.
                    InternalBufferOverflowException ovf => Observable.Throw<FileSystemEventArgs>(ovf),
                    //  Доступ к каталогу запрещён, возможно он был удален
                    Win32Exception fnfWin => Observable.Throw<FileSystemEventArgs>(
                        new FileNotFoundException("Каталог не существует", @this.Path, fnfWin)),
                    var exc => Observable.Throw<FileSystemEventArgs>(exc)
                }
            );

        return событияФС
            .Merge(ошибкиНаблюдения)
            .Finally(() => {
                @this.EnableRaisingEvents = false;
                @this.Dispose();
            })
            .Publish()
            .RefCount();
    }
}