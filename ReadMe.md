MemcachedTranscoder
======================

MemcachedTranscoder is custom transcoders of [Enyim.Memcached](https://github.com/enyim/EnyimMemcached). It serialize object using [Protobuf-net](http://code.google.com/p/protobuf-net/), [JSON.NET](http://json.codeplex.com/) and [MsgPack-Cli](https://github.com/msgpack/msgpack-cli).

Install
---
Install with NuGet.

```
Install-Package Enyim.Memcached.Transcoders.ProtocolBuffers
Install-Package Enyim.Memcached.Transcoders.Json
Install-Package Enyim.Memcached.Transcoders.MessagePack
Install-Package Enyim.Memcached.Transcoders.MessagePack.Map
```

Usage
---

edit transcoder type.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <configSections>
        <sectionGroup name="enyim.com">
            <section name="memcached" type="Enyim.Caching.Configuration.MemcachedClientSection, Enyim.Caching" />
        </sectionGroup>
    </configSections>
    <enyim.com>
        <memcached protocol="Binary">
            <servers>
                <add address="127.0.0.1" port="11211"/>
            </servers>
            <transcoder type="MemcachedTranscoder.ProtoTranscoder, ProtoTranscoder" />
        </memcached>
    </enyim.com>
</configuration>
```
transcoder variations

```xml
<transcoder type="MemcachedTranscoder.ProtoTranscoder, ProtoTranscoder" />
<transcoder type="MemcachedTranscoder.JsonTranscoder, JsonTranscoder" />
<transcoder type="MemcachedTranscoder.MessagePackTranscoder, MessagePackTranscoder" />
<transcoder type="MemcachedTranscoder.MessagePackMapTranscoder, MessagePackMapTranscoder" />
```
ProtoTranscoder using Protocol Buffers. protobuf-net has [official transcoder](http://nuget.org/packages/protobuf-net.Enyim).But it isn't support generics and primitive collections. This ProtoTranscoder supports.  
MessagePackTranscoder serialize array mode(default). MessagePackMapTranscoder serialize map mode.

Performance
---
Serialization, Deserialization speed and size test.  
Note::This isn't serializer test. This contains overhead of transcode.

```csharp
[ProtoContract]
[Serializable]
public class TestClass
{
    [ProtoMember(1)]
    [MessagePackMember(0)]
    public string MyProperty1 { get; set; }
    [ProtoMember(2)]
    [MessagePackMember(1)]
    public int MyProperty2 { get; set; }
    [ProtoMember(3)]
    [MessagePackMember(2)]
    public DateTime MyProperty3 { get; set; }
    [ProtoMember(4)]
    [MessagePackMember(3)]
    public bool MyProperty4 { get; set; }
}

// Simple POCO
var obj = new TestClass
{
    MyProperty1 = "hoge",
    MyProperty2 = 1,
    MyProperty3 = new DateTime(1999, 12, 11, 0, 0, 0, DateTimeKind.Utc),
    MyProperty4 = true
};

// Array
var array = Enumerable.Range(1, 10)
    .Select(i => new TestClass
    {
        MyProperty1 = "hoge" + i,
        MyProperty2 = i,
        MyProperty3 = new DateTime(1999, 12, 11, 0, 0, 0, DateTimeKind.Utc).AddDays(i),
        MyProperty4 = i % 2 == 0
    })
    .ToArray();
```

S is Serialize(millisecond)  
D is Deserialize(millisecond)  
Size is byte

```text
Simple POCO************************
S DefaultTranscoder:735
D DefaultTranscoder:750
Size:305
S DataContractTranscoder:775
D DataContractTranscoder:1642
Size:746
S ProtoTranscoder:99
D ProtoTranscoder:142
Size:88
S JsonTranscoder:772
D JsonTranscoder:892
Size:167
S MessagePackTranscoder:256
D MessagePackTranscoder:535
Size:89
S MessagePackMapTranscoder:327
D MessagePackMapTranscoder:783
Size:137

Array******************************
S DefaultTranscoder:4234
D DefaultTranscoder:4186
Size:712
S DataContractTranscoder:3874
D DataContractTranscoder:9532
Size:4525
S ProtoTranscoder:2189
D ProtoTranscoder:3040
Size:255
S JsonTranscoder:5618
D JsonTranscoder:6275
Size:1043
S MessagePackTranscoder:752
D MessagePackTranscoder:2696
Size:256
S MessagePackMapTranscoder:1453
D MessagePackMapTranscoder:5088
Size:736
```