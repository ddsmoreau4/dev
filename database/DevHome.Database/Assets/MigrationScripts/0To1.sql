﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "Repository" (
    "RepositoryId" INTEGER NOT NULL CONSTRAINT "PK_Repository" PRIMARY KEY AUTOINCREMENT,
    "RepositoryName" TEXT NOT NULL DEFAULT '',
    "RepositoryClonePath" TEXT NOT NULL DEFAULT '',
    "IsHidden" INTEGER NOT NULL,
    "ConfigurationFileLocation" TEXT NULL DEFAULT '',
    "RepositoryUri" TEXT NULL DEFAULT '',
    "SourceControlClassId" TEXT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "CreatedUTCDate" TEXT NULL DEFAULT (datetime()),
    "UpdatedUTCDate" TEXT NULL DEFAULT (datetime())
);

CREATE UNIQUE INDEX "IX_Repository_RepositoryName_RepositoryClonePath" ON "Repository" ("RepositoryName", "RepositoryClonePath");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240920200626_InitialMigration', '8.0.8');

COMMIT;

