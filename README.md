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
    let websiteTags = 
      [   ("Name", "l1x.be");   ("Environment", "website"); 
          ("Scope", "global");  ("Stage", "prod");         ]

    // dev.l1x.be

    let websiteDocuments : WebsiteDocuments = 
      { IndexDocument = "index.html"; ErrorDocument = "error.html"; }

    let s3BucketWithConfigDev = 
      createWebsiteBucketConfig "dev.l1x.be" "eu-west-1" "prod" websiteDocuments websiteTags

    createS3bucket amazonS3client s3BucketWithConfigDev |> ignore
    
    // redirect l1x.be -> dev.l1x.be

    let s3BucketWithConfigApex = 
      createRedirectBucketConfig "l1x.be" "eu-west-1" "prod" { RedirectTo = "dev.l1x.be" } websiteTags
    
    createS3bucket amazonS3client s3BucketWithConfigApex |> ignore
    
    // end 
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
    I->>N: getState
    I->>C: getState
    I->>E: getState
    N->>C: create
    N->>E: create
    C-->>N: destroy
    C->>E: destroy
    C->>C: update
    C->>E: update
```    

[![](https://mermaid.ink/img/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG4gICAgcGFydGljaXBhbnQgSSBhcyBJbml0aWFsXG4gICAgcGFydGljaXBhbnQgTiBhcyBOb25FeGlzdGVudFxuICAgIHBhcnRpY2lwYW50IEMgYXMgQ3JlYXRlZFxuICAgIHBhcnRpY2lwYW50IEUgYXMgRXJyXG4gICAgSS0-Pk46IGdldFN0YXRlXG4gICAgSS0-PkM6IGdldFN0YXRlXG4gICAgSS0-PkU6IGdldFN0YXRlXG4gICAgTi0-PkM6IGNyZWF0ZVxuICAgIE4tPj5FOiBjcmVhdGVcbiAgICBDLS0-Pk46IGRlc3Ryb3lcbiAgICBDLT4-RTogZGVzdHJveVxuICAgIEMtPj5DOiB1cGRhdGVcbiAgICBDLT4-RTogdXBkYXRlIiwibWVybWFpZCI6eyJ0aGVtZSI6ImRlZmF1bHQifX0)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG4gICAgcGFydGljaXBhbnQgSSBhcyBJbml0aWFsXG4gICAgcGFydGljaXBhbnQgTiBhcyBOb25FeGlzdGVudFxuICAgIHBhcnRpY2lwYW50IEMgYXMgQ3JlYXRlZFxuICAgIHBhcnRpY2lwYW50IEUgYXMgRXJyXG4gICAgSS0-Pk46IGdldFN0YXRlXG4gICAgSS0-PkM6IGdldFN0YXRlXG4gICAgSS0-PkU6IGdldFN0YXRlXG4gICAgTi0-PkM6IGNyZWF0ZVxuICAgIE4tPj5FOiBjcmVhdGVcbiAgICBDLS0-Pk46IGRlc3Ryb3lcbiAgICBDLT4-RTogZGVzdHJveVxuICAgIEMtPj5DOiB1cGRhdGVcbiAgICBDLT4-RTogdXBkYXRlIiwibWVybWFpZCI6eyJ0aGVtZSI6ImRlZmF1bHQifX0)

#### Api Gateway

#### Lambda

#### DynamoDB

#### Fargate