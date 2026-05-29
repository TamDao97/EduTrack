export interface ICurrentUser {
  id: string;
  userName: string;
  displayName: string;
  email: string;
  isAdmin: boolean;
  isSuper: boolean;
  roleName: string;
  accessToken: string;
  refreshToken: string;
  permissions: string[];
}
