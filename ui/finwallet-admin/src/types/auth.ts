export interface AuthResult {
  userId: string;
  email: string;
  accessToken: string | null;
  expiresAtUtc: string | null;
  requiresTotp: boolean;
  refreshToken: string | null;
  refreshExpiresAtUtc: string | null;
}

export interface TotpSetup {
  secret: string;
  otpAuthUri: string;
}
