export function getApiErrorMessage(error: unknown, fallback: string): string {
  const body = (error as { error?: Record<string, unknown> })?.error;
  if (!body) {
    return fallback;
  }

  const errors = body['errors'] ?? body['Errors'];
  if (errors && typeof errors === 'object') {
    const first = Object.values(errors as Record<string, string[]>)
      .flat()
      .find(Boolean);
    if (first) {
      return first;
    }
  }

  const message = (body['message'] ?? body['Message']) as string | undefined;
  return message ?? fallback;
}
