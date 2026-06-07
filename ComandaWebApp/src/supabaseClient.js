import { createClient } from '@supabase/supabase-js';

// Usamos las mismas credenciales que la app MAUI (AppConfig.cs)
const supabaseUrl = 'https://huqbrgygkviajpvgqgra.supabase.co';
const supabaseKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imh1cWJyZ3lna3ZpYWpwdmdxZ3JhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Nzk4NTUxOTMsImV4cCI6MjA5NTQzMTE5M30.7oyElPaVh4yq7SYZadU5xy4ebp4U5J7SpS8n3lAWzLM';

export const supabase = createClient(supabaseUrl, supabaseKey);
