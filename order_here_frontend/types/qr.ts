export interface QrResolveResponse {
  tableId: string;
  tableCode: string;
}

export interface ApiErrorResponse {
  errorCode: QrErrorCode;
  message: string;
  traceId?: string;
}

export type QrErrorCode =
  | "QR_INVALID"
  | "QR_NOT_FOUND"
  | "QR_INACTIVE"
  | "QR_EXPIRED"
  | "TABLE_NOT_FOUND"
  | "TABLE_INACTIVE"
  | "NOT_FOUND"
  | "UNEXPECTED_ERROR";
