using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    internal static bool IsPyAsyncGenerator(PyObject p) =>
        HasAttr(p, "__anext__") && HasAttr(p, "asend") && HasAttr(p, "aclose");
}
