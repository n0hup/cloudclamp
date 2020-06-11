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
      static member ToDomain (dto: AliasTargetDto) : AwsRoute53Resource.AliasTarget =
        { DNSName               = dto.DNSName
          EvaluateTargetHealth  = dto.EvaluateTargetHealth
          HostedZoneId          = dto.HostedZoneId }

  type AliasDto =
    {
      Name            : string
      Type            : string
      AliasTarget     : AliasTargetDto
    }
    with
      static member FromDomain (alias: AwsRoute53Resource.Alias) : AliasDto =
        let n = alias.Name
        let t = alias.Type
        let a = alias.AliasTarget
        {Name = n; Type = t.ToString(); AliasTarget = (AliasTargetDto.FromDomain a)}
      static member ToDomain (dto: AliasDto) : AwsRoute53Resource.Alias =
        { Name        = dto.Name
          Type        = (AwsRoute53Resource.fromStringToResourceRecordType dto.Type)
          AliasTarget = (AliasTargetDto.ToDomain dto.AliasTarget) }

  type RecordDto =
    {
      Name            : string
      Type            : string
      ResourceRecords : List<string>
      TTL             : uint32
    }
    with
      static member FromDomain (record: AwsRoute53Resource.Record) : RecordDto =
        let n   = record.Name
        let t   = record.Type
        let r   = record.ResourceRecords
        let ttl = record.TTL
        {Name = n; Type = t.ToString(); ResourceRecords = r; TTL = ttl}
      static member ToDomain (dto: RecordDto) : AwsRoute53Resource.Record =
        { Name            = dto.Name
          Type            = (AwsRoute53Resource.fromStringToResourceRecordType dto.Type)
          ResourceRecords = dto.ResourceRecords
          TTL             = dto.TTL }

  type ResourceRecordSetDto =
    {
      Tag         : string
      AliasData   : AliasDto
      RecordData  : RecordDto
    }
    with
      static member FromDomain (rrset: AwsRoute53Resource.ResourceRecordSet) : ResourceRecordSetDto =
        let nullAliasData   = Unchecked.defaultof<AliasDto>
        let nullRecordData  = Unchecked.defaultof<RecordDto>
        match rrset with
          | AwsRoute53Resource.ResourceRecordSet.Alias  a -> {Tag="AliasTarget";     AliasData=nullAliasData; RecordData = nullRecordData}
          | AwsRoute53Resource.ResourceRecordSet.Record r -> {Tag="ResourceRecords"; AliasData=nullAliasData; RecordData = nullRecordData}
      static member ToDomain (dto: ResourceRecordSetDto) : AwsRoute53Resource.ResourceRecordSet =
        match dto.Tag with
          | "AliasTarget"     -> AwsRoute53Resource.ResourceRecordSet.Alias(AliasDto.ToDomain dto.AliasData)
          | "ResourceRecords" -> AwsRoute53Resource.ResourceRecordSet.Record(RecordDto.ToDomain dto.RecordData)
          | x                 -> failwith (sprintf "%A is not supported" x)