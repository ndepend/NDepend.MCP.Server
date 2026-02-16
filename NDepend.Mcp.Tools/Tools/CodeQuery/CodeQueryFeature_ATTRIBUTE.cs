namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string ATTRIBUTE_PROMPT =
          """
          # ATTRIBUTES IN CQLINQ
          
          # OVERVIEW
          
          NDepend provides comprehensive support for querying attribute classes and code elements decorated with attributes.
          
          ## QUERYING ATTRIBUTES CLASSES
          
          Query all attribute classes (custom or built-in):
          ```csharp
          Types.Where(t => t.IsAttributeClass)
          ```
          
          ## QUERYING CODE ELEMENTS DECORATED WITH A SPECIFIC ATTRIBUTE
          
          The interface IAttributeTarget represents a code element that can be tagged with an attribute.
          It is implemented by IAssembly, IType, IMethod, IField, IProperty, and IEvent.
          
          IAttributeTarget has this methods and properties:
          ```csharp
          HasAttribute(IType attributeClass) // Returns true if this code element is tagged by the attributeClass
          HasAttribute(string attributeClassFullName) // Returns true if this code element is tagged by the attribute class specified by full name.
          IEnumerable<IAttributeTag> AttributeTagsOf(IType attributeClass) // Gets the IAttributeTag of type attributeClass tagging this code element.
          IEnumerable<IAttributeTag> AttributeTagsOnMe { get; } // Returns a sequence of attribute tags on this code element.
          IEnumerable<IType> AttributeClassesThatTagMe { get; } // Returns a sequence of attribute classes that tag this code element.
          ```
          
          ## QUERYING ATTRIBUTE TAGS
          
          The interface IAttributeTag represents an attribute instance that tags a code element.
          You can obtains an IAttributeTag instance from the various methods and properties of IAttributeTarget.
          
          ```csharp
          IAttributeTag has this methods and properties:
          IAttributeTarget CodeElementTagged { get; } // The code element tagged.
          IType AttributeType { get; } // Get the attribute class of the attribute tag.
          IValue ValueOf(string paramName) // Get the value of the parameter named "paramName".
          IReadOnlyList<IAttributeTagParameter> Parameters { get; } // Gets all parameters of this attribute tag.
          ```
          
          ## PRACTICAL EXAMPLES
          
          Example 1: Find Decorated Elements
          ```csharp
          // <Name>Types tagged with AttributeUsage</Name>
          from t in Application.Types
          // Notice the call to .AllowNoMatch() that is a marker to avoid query failure if the attribute is not found by full name
          where t.HasAttribute("System.AttributeUsageAttribute".AllowNoMatch())
          select t
          ```
          
          Example 2: Attribute Usage with Parameters
          ```csharp
          // <Name>System.AttributeUsageAttribute usage and parameters values</Name>
          let attributes = Types.WithFullName("System.AttributeUsageAttribute")
          where attributes.Any()
          
          let tags = attributes.SelectMany(sa => sa.TagsWithMeAsAttribute)
          from tag in tags 
          where tag.CodeElementTagged.IsType // Change or comment
          select new { 
            tag.CodeElementTagged, 
            nb_params = tag.Parameters.Count,
            @params = tag.Parameters.Select(p =>p.Name +": "+p.Value.ToString()).Aggregate("    "),
            tag.AttributeType  }
          ```
          
          Tips: Use Aggregate to concatenate strings from a sequence:
          `tag.Parameters.Select(p =>p.Name +": "+p.Value.AsString).Aggregate(",   ")`
          
          Example 3: Count Attribute Usages
          ```csharp
          // <Name>System.Diagnostics.DebuggerDisplayAttribute number of usage</Name>
          let attributes = ThirdParty.Types.
             WithFullName("System.Diagnostics.DebuggerDisplayAttribute")
          
          let tags = attributes.SelectMany(sa => sa.TagsWithMeAsAttribute)
          
          let taggeds = tags.ToLookup(t => t.CodeElementTagged)
          
          from tagged in taggeds
          select new { 
             tagged.Key , 
             nbTags = tagged.Count()
          }
          ```
          
          Advanced Example: Enforce Attribute Parameter Rules
          ```csharp
          // <Name>RouteAttribute template enforcement</Name>
          warnif count > 0  // This query is a rule with this prefix
          
          let attributes = ThirdParty.Types.WithName("RouteAttribute")
          where attributes.Any()
          
          let tags = attributes.SelectMany(sa => sa.TagsWithMeAsAttribute)
          from tag in tags 
          
          // ctor  RouteAttribute(string template)   Arg0 refers to the first argument
          let arg0 = tag.ValueOf("Arg0")  where arg0 != null
          let template = arg0.AsString 
          let violation =
          
          // should starts with api with no leading slash
          // KO: [Route("/api/products/")]     OK: [Route("api/products")] 
          !template.StartsWith("api/") ? 
            @"should starts with ""api/"" with no leading slash" :
          
          // Kebab-case enforcement: should not contain any upper character
          // KO: [Route("api/GetProducts")]    OK: [Route("api/get-products")]
          template.ToCharArray().Any(c => char.IsUpper(c)) ?
            "should not contain any upper character" :
          
          // Avoid reserved characters in route templates
          // KO: [Route("api/data&value")]
          template.ToCharArray().Any(c => "&=#?".Contains(c)) ?
            "should not contain reserver characters like '&=#?'" : null
          
          where !violation.IsNullOrEmpty()
          
          select new { 
            tag.CodeElementTagged, 
            template,
            violation,
            Debt = 5.ToMinutes().ToDebt()
          }
          //<Expl>
          // The *{0}* is tagged with RouteAttribute("{1}") but the template {2}.
          // </Expl>
          ```
          
          
          ## KEY PATTERNS
          
          Access Constructor Arguments by Position
          ```csharp
          attributeTag.ValueOf("Arg0")  // First parameter
          attributeTag.ValueOf("Arg1")  // Second parameter
          ```
          
          Access Named Parameters
          ```csharp
          attributeTag.ValueOf("AllowMultiple")  // Named parameter
          ```
          
          Get All Parameters
          ```csharp
          attributeTag.Parameters  // All parameters (positional + named)
          ```
          
          Navigate from Attribute Class to Tagged Elements
          ```csharp
          let attrType = Types.WithFullName("System.ObsoleteAttribute").Single()
          from tag in attrType.TagsWithMeAsAttribute  // All usages
          select tag.CodeElementTagged
          ```
          
          Filter by Element Type
          ```csharp
          from tag in attrType.TagsWithMeAsAttribute
          where tag.CodeElementTagged.IsMethod  // Only methods
          // or .IsType, .IsField, .IsProperty, .IsEvent
          select tag
          ```
          """;
}
