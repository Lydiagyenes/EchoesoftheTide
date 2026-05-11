// Ezt a kódrészletet azért kell hozzáadni, hogy a régebbi .NET verziók
// is megértsék a C# 9.0 'record' és 'init' kulcsszavait.
// Ez egy standard megoldás a Unity-ban.


namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}
