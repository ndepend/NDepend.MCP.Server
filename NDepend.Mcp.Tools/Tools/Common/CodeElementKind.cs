namespace NDepend.Mcp.Tools.Common; 
[Description("A flag enumeration that list kind of code elements available for searching")]
[Flags]
internal enum CodeElementKind  {
    Assembly = 0x01,
    Namespace = 0x02,
    Type = 0x04,
    Method = 0x08,
    Field = 0x10,
    Property = 0x20,
    Event = 0x40,

    All = Assembly | Namespace | Type | Method | Field | Property | Event,
    Member = Method | Field | Property | Event,
    None = 0
}