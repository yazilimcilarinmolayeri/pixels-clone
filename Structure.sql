/*
 This is the structure dump of the project
 Github: https://github.com/yazilimcilarinmolayeri/pixels
 Author: https://github.com/Berke-Alp
 
 --------------------DUMP INFORMATION-------------------
 Source Server         : Local PostgreSQL
 Source Server Type    : PostgreSQL
 Source Server Version : 140002

 Target Server Type    : PostgreSQL
 Target Server Version : 140002
 File Encoding         : 65001
 --------------------------------------------------------

*/

-- ----------------------------
-- Sequence structure for actions_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."actions_id_seq";
CREATE SEQUENCE "public"."actions_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for canvas_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."canvas_id_seq";
CREATE SEQUENCE "public"."canvas_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for pixel_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."pixel_id_seq";
CREATE SEQUENCE "public"."pixel_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Sequence structure for users_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."users_id_seq";
CREATE SEQUENCE "public"."users_id_seq" 
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
CACHE 1;

-- ----------------------------
-- Table structure for actions
-- ----------------------------
DROP TABLE IF EXISTS "public"."actions";
CREATE TABLE "public"."actions" (
  "id" int4 NOT NULL GENERATED ALWAYS AS IDENTITY (
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
),
  "userId" int4 NOT NULL,
  "pixelId" int4 NOT NULL,
  "actionDate" timestamp(0) NOT NULL DEFAULT CURRENT_TIMESTAMP(0)
)
;

-- ----------------------------
-- Table structure for canvas
-- ----------------------------
DROP TABLE IF EXISTS "public"."canvas";
CREATE TABLE "public"."canvas" (
  "id" int4 NOT NULL GENERATED ALWAYS AS IDENTITY (
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
),
  "sizeX" int4 NOT NULL,
  "sizeY" int4 NOT NULL,
  "isActive" bool NOT NULL DEFAULT true,
  "dateCreated" timestamp(0) NOT NULL DEFAULT CURRENT_TIMESTAMP(0),
  "dateClosed" timestamp(0),
  "dateExpire" timestamp(0)
)
;

-- ----------------------------
-- Table structure for pixel
-- ----------------------------
DROP TABLE IF EXISTS "public"."pixel";
CREATE TABLE "public"."pixel" (
  "id" int4 NOT NULL GENERATED ALWAYS AS IDENTITY (
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
),
  "canvasId" int4 NOT NULL,
  "xPos" int4 NOT NULL,
  "yPos" int4 NOT NULL,
  "color" int4 NOT NULL
)
;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS "public"."users";
CREATE TABLE "public"."users" (
  "id" int4 NOT NULL GENERATED ALWAYS AS IDENTITY (
INCREMENT 1
MINVALUE  1
MAXVALUE 2147483647
START 1
),
  "discordId" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "registerDate" timestamp(0) NOT NULL DEFAULT CURRENT_TIMESTAMP(0),
  "isBanned" bool NOT NULL DEFAULT false,
  "isModerator" bool NOT NULL DEFAULT false
)
;

-- ----------------------------
-- Function structure for SET_PIXEL
-- ----------------------------
DROP FUNCTION IF EXISTS "public"."SET_PIXEL"("p_canvasid" int4, "p_xpos" int4, "p_ypos" int4, "p_color" int4, OUT "pixel_id" int4);
CREATE OR REPLACE FUNCTION "public"."SET_PIXEL"(IN "p_canvasid" int4, IN "p_xpos" int4, IN "p_ypos" int4, IN "p_color" int4, OUT "pixel_id" int4)
  RETURNS "pg_catalog"."int4" AS $BODY$
 
 DECLARE result_count INTEGER;
 
 BEGIN
	
	UPDATE "pixel"
	SET "color" = p_color
	WHERE "xPos" = p_xpos AND "yPos" = p_ypos AND "canvasId" = p_canvasid
	RETURNING "id" INTO pixel_id;
	
	GET DIAGNOSTICS result_count = ROW_COUNT;
	
	IF result_count = 0 THEN
		INSERT INTO "pixel"
		VALUES(DEFAULT,p_canvasid,p_xpos,p_ypos,p_color)
		RETURNING "id" INTO pixel_id;
	END IF;

END$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

-- ----------------------------
-- View structure for GET_CURRENT_CANVAS
-- ----------------------------
DROP VIEW IF EXISTS "public"."GET_CURRENT_CANVAS";
CREATE VIEW "public"."GET_CURRENT_CANVAS" AS  SELECT canvas.id,
    canvas."sizeX",
    canvas."sizeY",
    canvas."isActive",
    canvas."dateCreated",
    canvas."dateClosed",
    canvas."dateExpire"
   FROM canvas
  WHERE canvas."isActive" = true AND canvas."dateClosed" IS NULL AND COALESCE(canvas."dateExpire"::timestamp with time zone, CURRENT_TIMESTAMP(0) + '1 year'::interval) > CURRENT_TIMESTAMP(0)
 LIMIT 1;

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "public"."actions_id_seq"
OWNED BY "public"."actions"."id";
SELECT setval('"public"."actions_id_seq"', 19, true);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "public"."canvas_id_seq"
OWNED BY "public"."canvas"."id";
SELECT setval('"public"."canvas_id_seq"', 2, true);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "public"."pixel_id_seq"
OWNED BY "public"."pixel"."id";
SELECT setval('"public"."pixel_id_seq"', 41, true);

-- ----------------------------
-- Alter sequences owned by
-- ----------------------------
ALTER SEQUENCE "public"."users_id_seq"
OWNED BY "public"."users"."id";
SELECT setval('"public"."users_id_seq"', 3, true);

-- ----------------------------
-- Primary Key structure for table actions
-- ----------------------------
ALTER TABLE "public"."actions" ADD CONSTRAINT "actions_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table canvas
-- ----------------------------
ALTER TABLE "public"."canvas" ADD CONSTRAINT "canvas_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Uniques structure for table pixel
-- ----------------------------
ALTER TABLE "public"."pixel" ADD CONSTRAINT "pixel_canvas_x_y" UNIQUE ("canvasId", "xPos", "yPos");

-- ----------------------------
-- Primary Key structure for table pixel
-- ----------------------------
ALTER TABLE "public"."pixel" ADD CONSTRAINT "pixel_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table users
-- ----------------------------
ALTER TABLE "public"."users" ADD CONSTRAINT "users_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table actions
-- ----------------------------
ALTER TABLE "public"."actions" ADD CONSTRAINT "actionPixel" FOREIGN KEY ("pixelId") REFERENCES "public"."pixel" ("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "public"."actions" ADD CONSTRAINT "actionUser" FOREIGN KEY ("userId") REFERENCES "public"."users" ("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- ----------------------------
-- Foreign Keys structure for table pixel
-- ----------------------------
ALTER TABLE "public"."pixel" ADD CONSTRAINT "pixel_canvas" FOREIGN KEY ("canvasId") REFERENCES "public"."canvas" ("id") ON DELETE CASCADE ON UPDATE CASCADE;
