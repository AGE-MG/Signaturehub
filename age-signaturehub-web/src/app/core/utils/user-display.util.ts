import { User } from '../models/user.model';

function sanitizeLogin(value?: string | null): string | null {
  if (!value) {
    return null;
  }

  const normalized = value.trim();
  if (!normalized) {
    return null;
  }

  if (normalized.includes('\\')) {
    return normalized.substring(normalized.lastIndexOf('\\') + 1);
  }

  if (normalized.includes('@')) {
    return normalized.substring(0, normalized.indexOf('@'));
  }

  return normalized;
}

export function getFriendlyUserName(user: User | null | undefined): string {
  const fullName = user?.fullName?.trim();

  if (fullName && !fullName.includes('\\') && !fullName.includes('@')) {
    return fullName;
  }

  const networkUserName = sanitizeLogin(user?.networkUserName);
  if (networkUserName) {
    return networkUserName;
  }

  const sanitizedFullName = sanitizeLogin(fullName);
  if (sanitizedFullName) {
    return sanitizedFullName;
  }

  const emailPrefix = sanitizeLogin(user?.email);
  if (emailPrefix) {
    return emailPrefix;
  }

  return 'Usuário';
}

export function getFriendlyUserInitials(user: User | null | undefined): string {
  const displayName = getFriendlyUserName(user);
  const parts = displayName.split(' ').filter(Boolean);

  if (parts.length === 0) {
    return '??';
  }

  if (parts.length === 1) {
    return parts[0].slice(0, 2).toUpperCase();
  }

  return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
}
