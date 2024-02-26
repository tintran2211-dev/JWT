import { IAuthUser, RolesEnum } from "../types/auth.types";
import axiosInstance from "../utils/axiosInstance";

export const setSession = (accessToken: string | null) => {
  if (accessToken) {
    localStorage.setItem("accessToken", accessToken);
    axiosInstance.defaults.headers.common.Authorization = `Bearer ${accessToken}`;
  } else {
    localStorage.removeItem("accessToken");
    delete axiosInstance.defaults.headers.common.Authorization;
  }
};

export const getSession = () => {
  return localStorage.getItem("accessToken");
};

export const allAccessRoles = [
  RolesEnum.OWNER,
  RolesEnum.ADMIN,
  RolesEnum.MANAGER,
  RolesEnum.USER,
];
export const managerAccessRoles = [
  RolesEnum.OWNER,
  RolesEnum.ADMIN,
  RolesEnum.MANAGER,
];
export const adminAccessRoles = [RolesEnum.OWNER, RolesEnum.ADMIN];
export const ownerAccessRoles = [RolesEnum.OWNER];

//Chỉ định vai trò người dùng có thể cập nhật khi người dùng đăng nhập
export const allowedRolesForUpdateArray = (
  loggedInUser?: IAuthUser
): string[] => {
  return loggedInUser?.roles.includes(RolesEnum.OWNER)
    ? [RolesEnum.ADMIN, RolesEnum.MANAGER, RolesEnum.USER]
    : [RolesEnum.MANAGER, RolesEnum.USER];
};

//Kiểm soát chủ sở hữu ko thể thay đổi vai trò chủ sở hữu
//Admin ko thể thay đổi vai trò admin và chủ sở hữu
export const isAuthorizedForUpdateRole = (
  loggedInUserRole: string,
  selectedUserRole: string
) => {
  let result = true;
  if (
    loggedInUserRole === RolesEnum.OWNER &&
    selectedUserRole === RolesEnum.OWNER
  ) {
    result = false;
  } else if (
    loggedInUserRole === RolesEnum.ADMIN &&
    (selectedUserRole === RolesEnum.OWNER ||
      selectedUserRole === RolesEnum.ADMIN)
  ) {
    result = false;
  }
  return result;
};
