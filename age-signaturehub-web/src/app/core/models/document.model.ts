export enum DocumentStatus {
  Draft = 0,
  PendingSignatures = 1,
  PartiallyCompleted = 2,
  Completed = 3,
  Rejected = 4,
  Expired = 5,
  Cancelled = 6
}

export enum DocumentSource {
  internal = 'internal',
  TJMG = 'TJMG',
  Tribunus = 'Tribunus',
  external = 'external'
}
