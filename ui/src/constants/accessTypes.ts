export const ACCESS_TYPES = [
  { value: 'Public', label: 'Public', description: 'All users can access' },
  { value: 'Private', label: 'Private', description: 'Only you can access' },
  { value: 'Restricted', label: 'Restricted', description: 'Only shared users can access' },
] as const;

export type AccessTypeValue = typeof ACCESS_TYPES[number]['value'];
