namespace CloudClamp

// external

// internal

module AwsAcmResource =

  type DomainName = string

  type ValidationMethod =
    | DNS

  type ValidationStatus =
    | SUCCESS

  type ResourceRecord = {
    Name    : string
    Type    : string
    Value   : string
  }

  type DomainValidationOptions = {
    DomainName      : DomainName
    ResourceRecord  : ResourceRecord
    ValidationDomain: DomainName
    ValidationMethod: ValidationMethod
    ValidationStatus: ValidationStatus
  }

  type ExtendedKeyUsageName =
    | TlsWebServerAuthentication
    | TlsWebClientAuthentication

  type ExtendedKeyUsage = {
    Name : ExtendedKeyUsageName
    OID  : string
  }

  type KeyAlgorithm =
    | RSA2048

  type KeyUsageName =
    | DigitalSsignature
    | KeyEncipherment

  type KeyUsage = {
    Name : KeyUsageName
  }

  type KeyUsages = List<KeyUsage>

  type Options = Map<string, string>

  type RenewalEligibility =
    | ELIGIBLE

  type SignatureAlgorithm =
    | SHA256WITHRSA

  type Status =
    | Issued

  type Type = AmazonIssued

  type CertificateResource = {
    CertificateArn            : string
    CreatedAt                 : string
    DomainName                : string
    DomainValidationOptions   : DomainValidationOptions
    ExtendedKeyUsages         : List<ExtendedKeyUsage>
    InUseBy                   : List<string>
    IssuedAt                  : string
    Issuer                    : string
    KeyAlgorithm              : KeyAlgorithm
    KeyUsages                 : KeyUsages
    NotAfter                  : string
    NotBefore                 : string
    Options                   : Options
    RenewalEligibility        : RenewalEligibility
    Serial                    : string
    SignatureAlgorithm        : SignatureAlgorithm
    Status                    : Status
    Subject                   : string
    SubjectAlternativeNames   : List<DomainName>
    Type                      : Type
  }
