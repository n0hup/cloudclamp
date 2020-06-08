namespace CloudClamp

// external
// internal
open AwsRoute53Resource
open Logging
open Config



module Blog =

  type Blog = {
    Dns         : List<AwsRoute53Resource.DnsResource>
    // Certificate : List<AwsAcmResource.CertificateResource>
    // Bucket      : List<AwsS3Resource.BucketResource>
  }

  let loggerBlog = Logger.CreateLogger "Website" loggingConfig.LogLevel

  let ns : ResourceRecordSet  =
    Record(
      "l1x.be."
      , NS
      ,[  "ns-583.awsdns-08.net."
          "ns-504.awsdns-63.com."
          "ns-1708.awsdns-21.co.uk."
          "ns-1176.awsdns-19.org." ]
      , 172800u
    )

  let soa : ResourceRecordSet =
    Record(
      "l1x.be."
      , SOA
      , ["ns-583.awsdns-08.net. awsdns-hostmaster.amazon.com. 1 7200 900 1209600 86400"]
      , 900u
    )

  let resourceRecordSets = [
    ns;
    soa;
    Alias(
      "dev.l1x.be.",
      A,
      { DNSName = "dbrgct5gwrbsd.cloudfront.net.";
        EvaluateTargetHealth = false;
        HostedZoneId = "Z2FDTNDATAQYW2" }
    );
    Alias(
      "dev.l1x.be.",
      AAAA,
      { DNSName = "dbrgct5gwrbsd.cloudfront.net.";
        EvaluateTargetHealth = false;
        HostedZoneId = "Z2FDTNDATAQYW2" }
    );
    Record(
      "_0a772de2cfcacc285d64f76d53afb931.dev.l1x.be."
      , CNAME
      , ["_031a48dd8cb38a7723ba15eeef0ae2b2.tfmgdnztqk.acm-validations.aws."]
      , 300u
    );
  ]

  let executeCommand command stage =
    let log = sprintf "command: %s stage: %s" command stage
    loggerBlog.LogInfo log

  // let blog : Blog =
  //   { Dns = dns }


// "module": "module.aws-acm-certs-trck-dev",
// "mode": "managed",
// "type": "aws_acm_certificate",
// "name": "cert",
// "provider": "provider.aws.us-east-1",
// "instances": [
//   {
//     "schema_version": 0,
//     "attributes": {
//       "arn": "arn:aws:acm:us-east-1:651831719661:certificate/7112ec11-b23b-42df-87f9-08c657666c71",
//       "certificate_authority_arn": "",
//       "certificate_body": null,
//       "certificate_chain": null,
//       "domain_name": "dev-trck.lambdainsight.com",
//       "domain_validation_options": [
//         {
//           "domain_name": "dev-trck.lambdainsight.com",
//           "resource_record_name": "_90f50843c005bddd7414f9abc8e8093b.dev-trck.lambdainsight.com.",
//           "resource_record_type": "CNAME",
//           "resource_record_value": "_059fbaf9e496135ae14993eb8800bd56.mzlfeqexyx.acm-validations.aws."
//         }
//       ],
//       "id": "arn:aws:acm:us-east-1:651831719661:certificate/7112ec11-b23b-42df-87f9-08c657666c71",
//       "options": [
//         {
//           "certificate_transparency_logging_preference": "ENABLED"