const ACCESS_TOKEN_KEY = 'finwallet_access_token';
const REFRESH_TOKEN_KEY = 'finwallet_refresh_token';
const EMAIL_KEY = 'finwallet_email';

export const storage = {
  setSession(accessToken: string, refreshToken: string, email: string) {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(EMAIL_KEY, email);
  },
  getAccessToken() {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  },
  getRefreshToken() {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  },
  getEmail() {
    return localStorage.getItem(EMAIL_KEY);
  },
  clear() {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
  }
};
