# cloudclamp

## AWS

### IAM

### ACM

### Route53

### S3

##### Bucket

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

### Api Gateway

### Lambda

### DynamoDB

### Fargate