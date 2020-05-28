# cloudclamp

Type safe infrastructure as code with the full power of F#.

## Why?

I am tired of dealing with configuration files and interpreters that has the expresiveness of Go, safetiness of C and performance of Ruby. Illegal configuration must be made impossible by the type system. ADTs are great for this. Compilation has to check as much as possible and using all language features is a must. C# has libraries for pretty much every single vendor out there or it is trivial to implement the lacking support. There is a giant community of senior software engineers available on StackOverflow or LinkedIN. Debugging and performance tracing is largely solved.

## Usage

CloudClamp has 3 concepts:

- service (website, hadoop, etcd, yourcustomservice)
- stage (dev, qa, prod, etc.)
- command (show, plan, deploy)

```bash
cloudclamp --stage prod --command deploy --service CloudClamp.Website
```

Right now there is no local state but this might change in the future. State must be per service/stage to avoid deployments blocking each other. There is a small amount of configuration in JSON (type safe) to configure basic things. More complex things live in code.

### Example resource (website)

This website uses AWS S3. It only has one stage: prod. The bucket configuration is typed. You cannot accidentally try to create illegal configuration. Possible configurations can be narrowed down by the actual company or department. For example, public buckets can be disabled. Tagging is flexible, you can add more for billing breakdown purposes.

```Fsharp
  // Tags

    let websiteTags = 
      [   ("Name", "l1x.be");   ("Environment", "website"); 
          ("Scope", "global");  ("Stage", "prod");         ]

    // logs.l1x.be

    let s3BucketWithConfigLogs = 
      createPrivateBucketConfig 
        "logs.l1x.be"     // name
        "eu-west-1"       // region
        "prod"            // stage
        websiteTags       // tagging
        None              // policy
        None              // logging
    
    createS3Bucket amazonS3client s3BucketWithConfigLogs |> ignore
    
    // dev.l1x.be

    let websiteDocuments : WebsiteDocuments = 
      { IndexDocument = "index.html"; ErrorDocument = "error.html"; }  

    let s3BucketWithConfigDev = 
      createWebsiteBucketConfig 
        "dev.l1x.be"        // name
        "eu-west-1"         // region
        "prod"              // stage
        websiteDocuments    // website
        websiteTags         // tagging
        None                // policy
        None                // logging

    createS3Bucket amazonS3client s3BucketWithConfigDev |> ignore
    
    // redirect l1x.be -> dev.l1x.be

    let redirectTo : RedirectOnly = 
      { RedirectTo = "dev.l1x.be" }

    let s3BucketWithConfigApex = 
      createRedirectBucketConfig 
        "l1x.be"          // name
        "eu-west-1"       // region
        "prod"            // stage
        redirectTo        // website
        websiteTags       // tagging
        None              // policy
        None              // logging
   
    createS3Bucket amazonS3client s3BucketWithConfigApex |> ignore
```

## Cloud Resources

### AWS

#### IAM

#### ACM

#### Route53

#### S3

###### Bucket

Bucket life cycle:

```
sequenceDiagram
    participant I as Initial
    participant N as NonExistent
    participant C as Created
    participant E as Err

    I->>C: getState
    I->>E: getState
    I->>N: getState

    N->>C: putBucket
    N->>E: putBucket

    C->>C:   PutBucketTagging
    C->>E:   PutBucketTagging

    C->>C:  PutBucketWebsite
    C->>E:   PutBucketWebsite

    C->>C:  PutBucketPolicy
    C->>E:   PutBucketPolicy

    C-->>C: DeleteBucketTagging
    C-->>E: DeleteBucketTagging

    C-->>C: DeleteBucketWebsite
    C-->>E:  DeleteBucketWebsite  
    
    C-->>C: DeleteBucketPolicy
    C-->>E:  DeleteBucketPolicy  

    C-->>N: deleteBucket
    C-->>E: deleteBucket
```    

[![](https://mermaid.ink/img/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG4gICAgcGFydGljaXBhbnQgSSBhcyBJbml0aWFsXG4gICAgcGFydGljaXBhbnQgTiBhcyBOb25FeGlzdGVudFxuICAgIHBhcnRpY2lwYW50IEMgYXMgQ3JlYXRlZFxuICAgIHBhcnRpY2lwYW50IEUgYXMgRXJyXG5cbiAgICBJLT4-QzogZ2V0U3RhdGVcbiAgICBJLT4-RTogZ2V0U3RhdGVcbiAgICBJLT4-TjogZ2V0U3RhdGVcblxuICAgIE4tPj5DOiBwdXRCdWNrZXRcbiAgICBOLT4-RTogcHV0QnVja2V0XG5cbiAgICBDLT4-QzogICBQdXRCdWNrZXRUYWdnaW5nXG4gICAgQy0-PkU6ICAgUHV0QnVja2V0VGFnZ2luZ1xuXG4gICAgQy0-PkM6ICBQdXRCdWNrZXRXZWJzaXRlXG4gICAgQy0-PkU6ICAgUHV0QnVja2V0V2Vic2l0ZVxuXG4gICAgQy0-PkM6ICBQdXRCdWNrZXRQb2xpY3lcbiAgICBDLT4-RTogICBQdXRCdWNrZXRQb2xpY3lcblxuICAgIEMtLT4-QzogRGVsZXRlQnVja2V0VGFnZ2luZ1xuICAgIEMtLT4-RTogRGVsZXRlQnVja2V0VGFnZ2luZ1xuXG4gICAgQy0tPj5DOiBEZWxldGVCdWNrZXRXZWJzaXRlXG4gICAgQy0tPj5FOiAgRGVsZXRlQnVja2V0V2Vic2l0ZSAgXG4gICAgXG4gICAgQy0tPj5DOiBEZWxldGVCdWNrZXRQb2xpY3lcbiAgICBDLS0-PkU6ICBEZWxldGVCdWNrZXRQb2xpY3kgIFxuXG4gICAgQy0tPj5OOiBkZWxldGVCdWNrZXRcbiAgICBDLS0-PkU6IGRlbGV0ZUJ1Y2tldFxuIiwibWVybWFpZCI6eyJ0aGVtZSI6ImRlZmF1bHQifSwidXBkYXRlRWRpdG9yIjpmYWxzZX0)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG4gICAgcGFydGljaXBhbnQgSSBhcyBJbml0aWFsXG4gICAgcGFydGljaXBhbnQgTiBhcyBOb25FeGlzdGVudFxuICAgIHBhcnRpY2lwYW50IEMgYXMgQ3JlYXRlZFxuICAgIHBhcnRpY2lwYW50IEUgYXMgRXJyXG5cbiAgICBJLT4-QzogZ2V0U3RhdGVcbiAgICBJLT4-RTogZ2V0U3RhdGVcbiAgICBJLT4-TjogZ2V0U3RhdGVcblxuICAgIE4tPj5DOiBwdXRCdWNrZXRcbiAgICBOLT4-RTogcHV0QnVja2V0XG5cbiAgICBDLT4-QzogICBQdXRCdWNrZXRUYWdnaW5nXG4gICAgQy0-PkU6ICAgUHV0QnVja2V0VGFnZ2luZ1xuXG4gICAgQy0-PkM6ICBQdXRCdWNrZXRXZWJzaXRlXG4gICAgQy0-PkU6ICAgUHV0QnVja2V0V2Vic2l0ZVxuXG4gICAgQy0-PkM6ICBQdXRCdWNrZXRQb2xpY3lcbiAgICBDLT4-RTogICBQdXRCdWNrZXRQb2xpY3lcblxuICAgIEMtLT4-QzogRGVsZXRlQnVja2V0VGFnZ2luZ1xuICAgIEMtLT4-RTogRGVsZXRlQnVja2V0VGFnZ2luZ1xuXG4gICAgQy0tPj5DOiBEZWxldGVCdWNrZXRXZWJzaXRlXG4gICAgQy0tPj5FOiAgRGVsZXRlQnVja2V0V2Vic2l0ZSAgXG4gICAgXG4gICAgQy0tPj5DOiBEZWxldGVCdWNrZXRQb2xpY3lcbiAgICBDLS0-PkU6ICBEZWxldGVCdWNrZXRQb2xpY3kgIFxuXG4gICAgQy0tPj5OOiBkZWxldGVCdWNrZXRcbiAgICBDLS0-PkU6IGRlbGV0ZUJ1Y2tldFxuIiwibWVybWFpZCI6eyJ0aGVtZSI6ImRlZmF1bHQifSwidXBkYXRlRWRpdG9yIjpmYWxzZX0)

#### Api Gateway

#### Lambda

#### DynamoDB

#### Fargate