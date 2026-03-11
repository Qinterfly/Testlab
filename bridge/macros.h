
#ifdef BRIDGE_EXPORTS
#define BRIDGE_EXPORTS __declspec(dllexport)
#else
#define BRIDGE_EXPORTS __declspec(dllimport)
#endif
