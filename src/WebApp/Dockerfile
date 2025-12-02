FROM node:20-alpine AS build
WORKDIR /app

# Copiar package.json e instalar dependências
COPY package*.json ./
RUN npm install --registry=https://registry.npmjs.org/

# Copiar código e fazer build
COPY . .
RUN npm run build

# Stage final - servir com Node.js
FROM node:20-alpine
WORKDIR /app

# Instalar serve para servir arquivos estáticos
RUN npm install -g serve

# Copiar build do Angular
COPY --from=build /app/dist/webapp/browser /app/dist

# Renomear index.csr.html para index.html (Angular 18 SSR)
RUN if [ -f /app/dist/index.csr.html ]; then mv /app/dist/index.csr.html /app/dist/index.html; fi

EXPOSE 80

# Servir a aplicação como SPA (Single Page Application)
CMD ["serve", "-s", "/app/dist", "-l", "80", "--no-clipboard"]
