namespace System.Threading.Tasks;

public static class TaskРасширения {
    public static void НеЖдать(this Task задача) { }

    public static void НеЖдать(this ValueTask задача) { }
}