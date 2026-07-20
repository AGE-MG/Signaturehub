export interface ExternalServiceConnection {
  id: string;
  name: string;
  url: string;
  events: string[];
  isActive: boolean;
  lastDeliveryAt?: string;
  lastDeliverySucceeded?: boolean;
  secret?: string;
}

export interface SaveExternalServiceConnection {
  name: string;
  url: string;
  events: string[];
}
