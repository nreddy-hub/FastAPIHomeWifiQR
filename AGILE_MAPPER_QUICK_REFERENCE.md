# ?? AgileMapper Quick Reference Card

## ?? Dependency Injection

```csharp
public MyEndpoint(IObjectMapper mapper)
{
    _mapper = mapper;
}
```

---

## ?? Common Mapping Patterns

### **Pattern 1: Single Object**
```csharp
var destination = _mapper.Map<DestinationType>(source);
```

**Example:**
```csharp
var response = _mapper.Map<WifiNetworkResponse>(network);
```

---

### **Pattern 2: Collection**
```csharp
var destinations = sources.Select(s => _mapper.Map<DestinationType>(s)).ToList();
```

**Example:**
```csharp
var responses = networks.Select(n => _mapper.Map<WifiNetworkResponse>(n)).ToList();
```

---

### **Pattern 3: Partial Manual (When Needed)**
```csharp
var destination = _mapper.Map<DestinationType>(source);
destination.CustomProperty = CalculateValue();
```

**Example:**
```csharp
var message = new WifiQrCreatedMessage
{
    WifiId = created.Id,  // Manual: property name differs
    Ssid = created.Ssid,
    Encryption = created.Encryption,
    Hidden = created.Hidden,
    CreatedAt = DateTime.UtcNow,  // Calculated
    CreatedBy = User?.Identity?.Name ?? "System"  // Runtime value
};
```

---

## ? When to Use AgileMapper

| Scenario | Use AgileMapper? | Example |
|----------|------------------|---------|
| Property names match | ? Yes | `Ssid ? Ssid` |
| Simple types | ? Yes | `string ? string`, `int ? int` |
| Collections | ? Yes | `List<A> ? List<B>` |
| Same property count | ? Yes | Both have 5 properties |
| Property name differs | ? No | `Id ? WifiId` (manual) |
| Calculated values | ? No | `DateTime.UtcNow` (manual) |
| Business logic | ? No | Token generation (manual) |
| Binary data | ? No | `byte[]` files (no mapping) |

---

## ?? Your Endpoints

### **? Using AgileMapper**

```
CreateWifiEndpoint
?? CreateWifiRequest ? WifiNetwork
?? WifiNetwork ? CreateWifiResponse

GetAllWifiEndpoint
?? List<WifiNetwork> ? List<WifiNetworkResponse>

GetWifiByIdEndpoint
?? WifiNetwork ? WifiNetworkResponse
```

### **? Not Using (Not Applicable)**

```
LoginEndpoint (business logic)
DownloadQrCodeEndpoint (binary data)
DownloadBulkQrCodesEndpoint (binary data)
```

---

## ?? Cheat Sheet

```csharp
// Single object
var dto = _mapper.Map<DtoType>(entity);

// Collection
var dtos = entities.Select(e => _mapper.Map<DtoType>(e)).ToList();

// Update existing
_mapper.Map(source).Over(existingDestination);

// With configuration (advanced)
var dto = _mapper.Map<DtoType>(entity).Using(config => {
    config.IgnoreTargetMembersWhere(m => m.Name == "Password");
});
```

---

## ?? Pro Tips

1. **Dependency Injection:** Always inject `IObjectMapper`, not concrete type
2. **Naming:** Keep DTO property names matching domain model
3. **Testing:** Test mappings in unit tests
4. **Performance:** AgileMapper caches compiled mappings (very fast)
5. **Manual When Needed:** Don't force AgileMapper if manual is clearer

---

## ?? More Info

See `AGILE_MAPPER_USAGE.md` for detailed documentation.
