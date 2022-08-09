// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Modules.EcsLite {
    public struct EcsPackedEntity {
        internal int Id;
        internal int Gen;
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
}