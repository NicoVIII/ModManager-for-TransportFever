namespace TPFModManager

type TPFMMError =
    // TransportFeverNet - Site
    | NoConnection
    // TransportFeverNet - Name
    | UnsupportedLayout
    // TransportFeverNet - Version
    | NoVersionOnWebsite
    // TransportFeverNet - FilePath
    | MoreThanOneFile
    | NotSupportedFormat of string
    // TransportFeverNet - FileBytes
    | NotBinary

    // Extract
    | ModInvalid
    | ExtractionFailed

    // Installation
    | AlreadyInstalled