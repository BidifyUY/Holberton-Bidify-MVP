#!/usr/bin/env node

import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { dirname } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Configuration
const CONFIG = {
  swaggerUrl: process.env.SWAGGER_URL || 'http://localhost:5015/swagger/v1/swagger.json',
  outputDir: '../api-generated',
  tempFile: './swagger-spec.json',
};

// Colors for console output
const colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  red: '\x1b[31m',
  blue: '\x1b[34m',
};

function log(message, color = 'reset') {
  console.log(`${colors[color]}${message}${colors.reset}`);
}

async function downloadSwaggerSpec() {
  log('📥 Downloading Swagger specification...', 'blue');

  try {
    const response = await fetch(CONFIG.swaggerUrl);
    if (!response.ok) {
      throw new Error(`Failed to fetch Swagger spec: ${response.statusText}`);
    }

    const spec = await response.json();

    // Save to temp file
    fs.writeFileSync(CONFIG.tempFile, JSON.stringify(spec, null, 2));
    log('✅ Swagger specification downloaded successfully', 'green');

    return spec;
  } catch (error) {
    log(`❌ Error downloading Swagger spec: ${error.message}`, 'red');
    log('Make sure your backend server is running on http://localhost:5015', 'yellow');
    process.exit(1);
  }
}

function generateTypes() {
  log('🔨 Generating TypeScript types...', 'blue');

  try {
    // Clean output directory
    if (fs.existsSync(CONFIG.outputDir)) {
      fs.rmSync(CONFIG.outputDir, { recursive: true, force: true });
    }

    // Generate types using openapi-typescript-codegen
    const command = `npx openapi-typescript-codegen --input ${CONFIG.tempFile} --output ${CONFIG.outputDir} --client axios --useOptions true --useUnionTypes true --exportCore true --exportServices true --exportModels true`;

    execSync(command, { stdio: 'inherit' });

    log('✅ TypeScript types generated successfully', 'green');
  } catch (error) {
    log(`❌ Error generating types: ${error.message}`, 'red');
    process.exit(1);
  }
}

function updateGeneratedFiles() {
  log('📝 Updating generated files...', 'blue');

  try {
    // Update the OpenAPI configuration in generated files
    const openApiPath = path.join(CONFIG.outputDir, 'core', 'OpenAPI.ts');

    if (fs.existsSync(openApiPath)) {
      let content = fs.readFileSync(openApiPath, 'utf-8');

      // Update BASE URL to use environment variable
      content = content.replace(
        /BASE:\s*'[^']*'/,
        "BASE: import.meta.env.VITE_API_URL || 'http://localhost:5015'"
      );

      fs.writeFileSync(openApiPath, content);
      log('✅ Updated OpenAPI configuration', 'green');
    }
  } catch (error) {
    log(`⚠️  Warning: Could not update generated files: ${error.message}`, 'yellow');
  }
}

function cleanup() {
  // Remove temporary file
  if (fs.existsSync(CONFIG.tempFile)) {
    fs.unlinkSync(CONFIG.tempFile);
  }
}

async function main() {
  log('\n🚀 Starting API types generation...', 'bright');

  try {
    await downloadSwaggerSpec();
    generateTypes();
    updateGeneratedFiles();

    log('\n✨ API types generation completed successfully!', 'green');
    log(`📁 Generated files are in: ${CONFIG.outputDir}`, 'blue');
    log('\n💡 You can now import types and services from:', 'yellow');
    log("   import { YourService, YourModel } from '../api-generated';", 'bright');

  } catch (error) {
    log(`\n❌ Generation failed: ${error.message}`, 'red');
    process.exit(1);
  } finally {
    cleanup();
  }
}

// Run the script
main();
