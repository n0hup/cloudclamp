namespace CloudClamp

// external

// internal

module AwsRoute53Dto =

  // ResourceRecordSets:
  //   - Name: l1x.be.
  //     ResourceRecords:
  //     - Value: ns-583.awsdns-08.net.
  //     - Value: ns-504.awsdns-63.com.
  //     - Value: ns-1708.awsdns-21.co.uk.
  //     - Value: ns-1176.awsdns-19.org.
  //     TTL: 172800
  //     Type: NS
  //   - Name: l1x.be.
  //     ResourceRecords:
  //     - Value: ns-583.awsdns-08.net. awsdns-hostmaster.amazon.com. 1 7200 900 1209600
  //         86400
  //     TTL: 900
  //     Type: SOA
  //   - AliasTarget:
  //       DNSName: dbrgct5gwrbsd.cloudfront.net.
  //       EvaluateTargetHealth: false
  //       HostedZoneId: Z2FDTNDATAQYW2
  //     Name: dev.l1x.be.
  //     Type: A
  //   - AliasTarget:
  //       DNSName: dbrgct5gwrbsd.cloudfront.net.
  //       EvaluateTargetHealth: false
  //       HostedZoneId: Z2FDTNDATAQYW2
  //     Name: dev.l1x.be.
  //     Type: AAAA
  //   - Name: _0a772de2cfcacc285d64f76d53afb931.dev.l1x.be.
  //     ResourceRecords:
  //     - Value: _031a48dd8cb38a7723ba15eeef0ae2b2.tfmgdnztqk.acm-validations.aws.
  //     TTL: 300
  //     Type: CNAME

  type AliasTargetDto =
    {
      DNSName               : string
      EvaluateTargetHealth  : bool
      HostedZoneId          : string
    }
    with
      static member FromDomain (aliasTarget: AwsRoute53Resource.AliasTarget ) : AliasTargetDto =
        let d = aliasTarget.DNSName
        let e = aliasTarget.EvaluateTargetHealth
        let h = aliasTarget.HostedZoneId
        {DNSName = d; EvaluateTargetHealth = e; HostedZoneId = h}
      static member ToDomain (dto: AliasTargetDto) : Result<AwsRoute53Resource.AliasTarget, string> =
        Ok {DNSName = dto.DNSName; EvaluateTargetHealth = dto.EvaluateTargetHealth; HostedZoneId = dto.HostedZoneId}

  type ResourceRecordTypeDto =
    string

  type AliasDto =
    {
      Name            : string
      Type            : ResourceRecordTypeDto
      AliasTarget     : AliasTargetDto
    }

  type RecordDto =
    {
        Name            : string
        Type            : ResourceRecordTypeDto
        ResourceRecords : List<string>
        TTL             : uint32
    }

  type ResourceRecordSetDto =
    {
      Tag: string
      AliasData: AliasDto
      RecordData: RecordDto
    }

