// During development, use your machine's LAN IP address.
// The emulator/phone cannot reach "localhost" on your dev machine.
// Find your IP: ipconfig (Windows) or ifconfig (Mac/Linux)
export const API_BASE_URL = __DEV__
  ? 'http://172.20.10.2:5117'
  : 'https://your-production-url.com';
