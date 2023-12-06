
---------drop database originalstoremada;
---------create database originalstoremada;
------ xesadsadas 


DROP TABLE divers_entrer;
DROP TABLE type_entrer;
DROP TABLE divers_depense;
DROP TABLE type_depense;

-------------------------------

drop table interaction_event;
drop table produit_event;
drop table evenement;
-------------------
drop table adresse_livraisonfacture;
drop table payement_produit;
drop table facture;
---------------
drop table adresse_livraisondevis;
drop table payement_devis;
drop table type_payement;
-----------------
drop table caisse_devis;
drop table caisse_boutique;
----------------
drop table vol_devis;
drop table commande_devis;
drop table devis;
drop table vol;
----------------------
drop table entree_produit;
drop table sortie_produit;
drop table favoris_produit;
drop table preference_produit;
drop table promotion_produit;    
drop table prix_produit;    
drop table contenue_produit;
drop table produit;
drop table type_produit;
drop table commission;
drop table frais_importation;
drop table categorie_produit;
-------------------
drop table affectation_employer;
drop table boutique;

CREATE EXTENSION IF NOT EXISTS pgcrypto;


CREATE TABLE admin(
    id serial primary key ,
    mail VARCHAR(100) not NULL unique,
    nom VARCHAR(50) NOT NULL ,
    prenom VARCHAR(50) NOT NULL ,
    adresse VARCHAR(80) ,
    code VARCHAR(100) NOT NULL ,
    niveau int default 0
);

CREATE TABLE client(
    id bigserial primary key ,
    mail VARCHAR(100) not NULL unique,
    nom VARCHAR(50) NOT NULL ,
    prenom VARCHAR(50) NOT NULL ,
    adresse VARCHAR(80) ,
    code VARCHAR(100) NOT NULL
);

SELECT * FROM client WHERE code = crypt('client1', code);

-- CREATE TABLE info_livraison(
--     id bigserial primary key ,
--     type varchar(50), 
--     montant decimal (12,2) , 
--     date timestamp default now()
-- );

CREATE TABLE cours_euro(
   id bigserial PRIMARY KEY ,
   montant_ariary decimal(18,2) CHECK ( montant_ariary >=0 ),
   date TIMESTAMP DEFAULT NOW()
);

CREATE TABLE home_img(
    id serial primary key ,
    title varchar(200) ,
    sub_title varchar(200) ,
    image text 
);

-----------------------------------------------------------
-----------------------BOUTIQUE----------------------------
-----------------------------------------------------------

CREATE TABLE boutique(
    id serial PRIMARY KEY ,
    description text ,
    nom VARCHAR(100) ,
    ville VARCHAR(30) ,
    quartier VARCHAR(30) ,
    adresse VARCHAR(50) unique ,
    longitude double precision ,
    latitude double precision ,
    unique (longitude, latitude)
);

CREATE TABLE param_frais_livraison(
    id serial PRIMARY KEY ,
    max_rayon double precision,
    prixparkm_ar decimal(8,2),
    prixmin_ar decimal(8,2)
);

CREATE TABLE affectation_employer(
    id serial PRIMARY KEY ,
    id_admin int not NULL ,
    id_boutique int Not NULL,
    datedeb TIMESTAMP not null DEFAULT now(),
    datefin TIMESTAMP DEFAULT null
);

ALTER TABLE affectation_employer add FOREIGN KEY(id_admin) REFERENCES admin(id);
ALTER TABLE affectation_employer add FOREIGN KEY(id_boutique) REFERENCES boutique(id);

-----------------------------------------------------------
-----------------------PRODUITS----------------------------
-----------------------------------------------------------

CREATE TABLE categorie_produit(
    id serial PRIMARY KEY ,
    nom VARCHAR(20) ,
    code varchar(6) unique 
);

CREATE TABLE frais_importation(
    id bigserial PRIMARY KEY ,
    id_categorie int not null ,
    montant_euro decimal(18,2) CHECK( montant_euro>=0) ,
    datedeb TIMESTAMP DEFAULT now() ,
    datefin TIMESTAMP DEFAULT null
);

ALTER TABLE frais_importation ADD FOREIGN KEY(id_categorie) REFERENCES categorie_produit(id);

CREATE TABLE commission(
    id bigserial PRIMARY KEY ,
    id_categorie int not null ,
    montant_euro decimal(18,2) CHECK ( montant_euro>=0) ,
    datedeb TIMESTAMP DEFAULT now() ,
    datefin TIMESTAMP DEFAULT null
);

ALTER TABLE commission ADD FOREIGN KEY(id_categorie) REFERENCES categorie_produit(id);

CREATE TABLE type_produit(
    id serial primary key ,
    nom VARCHAR(100) unique 
);

CREATE TABLE produit(
    id bigserial PRIMARY KEY ,
    id_categorie int NOT NULL ,
    id_type int not null ,
    pour_enfant bool default false , 
    nom VARCHAR(150) ,
    description text ,
    fournisseur VARCHAR(20) ,
    unique (id_categorie,id_type,pour_enfant,nom,fournisseur,description)
);

ALTER TABLE produit ADD FOREIGN KEY(id_categorie) REFERENCES categorie_produit(id);
ALTER TABLE produit ADD FOREIGN KEY(id_type) REFERENCES type_produit(id);

CREATE TABLE contenue_produit(
    id bigserial PRIMARY KEY ,
    id_produit bigint not NULL,
    image text ,
    couleur varchar(100) ,
    isprincipal boolean default false
);

ALTER TABLE contenue_produit ADD FOREIGN KEY(id_produit) REFERENCES produit(id);

CREATE TABLE prix_produit(
    id bigserial PRIMARY KEY ,
    id_produit bigint not NULL ,
    prix_achat decimal(18,2) CHECK(prix_achat > 0) ,
    prix_vente decimal(18,2) CHECK(prix_vente > 0) ,
    datedeb TIMESTAMP DEFAULT NOW() ,
    datefin TIMESTAMP DEFAULT null
);

ALTER TABLE prix_produit ADD FOREIGN KEY(id_produit) REFERENCES produit(id);
ALTER TABLE prix_produit ADD FOREIGN KEY(id_contenue) REFERENCES contenue_produit(id);

CREATE TABLE promotion_produit(
    id bigserial primary key ,
    id_produit bigint not null ,
    pourcentage float not null ,
    datedeb TIMESTAMP DEFAULT NOW() ,
    datefin TIMESTAMP DEFAULT null
);

ALTER TABLE promotion_produit ADD FOREIGN KEY(id_produit) REFERENCES produit(id);

-- CREATE TABLE demande_apro(
--     id serial primary key ,
--     id_
-- );

CREATE TABLE preference_produit(
    id bigserial PRIMARY KEY,
    id_produit bigint NOT NULL ,
    taille varchar(5) not null ,
    id_contenue bigint not null ,
    unique (id_produit, taille, couleur)
);

ALTER TABLE preference_produit ADD FOREIGN KEY(id_produit) REFERENCES produit(id);
ALTER TABLE preference_produit ADD FOREIGN KEY(id_contenue) REFERENCES contenue_produit(id);

CREATE TABLE favoris_produit(
    id bigserial PRIMARY KEY, 
    id_client bigint not null ,
    id_produit bigint not null ,
    date timestamp not null ,
    unique (id_client , id_produit)
);

ALTER TABLE favoris_produit ADD FOREIGN KEY(id_client) REFERENCES client(id);
ALTER TABLE favoris_produit ADD FOREIGN KEY(id_produit) REFERENCES produit(id);

CREATE TABLE entree_produit(
    id bigserial PRIMARY KEY ,
    id_boutique int not NULL ,
    id_produit bigint not null ,
    id_preferenceproduit bigint NOT NULL ,
    quantiter int check (quantiter > 0 ) ,
    date TIMESTAMP DEFAULT null
);

ALTER TABLE entree_produit ADD FOREIGN KEY(id_boutique) REFERENCES boutique(id);
ALTER TABLE entree_produit ADD FOREIGN KEY(id_produit) REFERENCES produit(id);
ALTER TABLE entree_produit ADD FOREIGN KEY(id_preferenceproduit) REFERENCES preference_produit(id);

CREATE TABLE sortie_produit(
    id bigserial PRIMARY KEY ,
    id_boutique int not NULL ,
    id_produit bigint not null ,
    id_preferenceproduit bigint NOT NULL ,
    quantiter int check (quantiter > 0 ) ,
    date TIMESTAMP DEFAULT null 
);

ALTER TABLE sortie_produit ADD FOREIGN KEY(id_boutique) REFERENCES boutique(id);
ALTER TABLE sortie_produit ADD FOREIGN KEY(id_produit) REFERENCES produit(id);
ALTER TABLE sortie_produit ADD FOREIGN KEY(id_preferenceproduit) REFERENCES preference_produit(id);

-----------------------------------------------------------
-----------------------DEVIS_VOL----------------------------
-----------------------------------------------------------

CREATE TABLE vol(
    id bigserial PRIMARY KEY,
    datedepart  TIMESTAMP not NULL ,
    datearriver_estimmer TIMESTAMP not NULL ,
    datearriver TIMESTAMP default null,
);

CREATE TABLE devis(
    id bigserial PRIMARY KEY ,
    id_client bigint NOT NULL ,
    date TIMESTAMP DEFAULT NOW() ,
    cours_devis decimal(8,2) default null,
    frais_importation decimal(8,2) default null,
    commission decimal(8,2) default null,
    date_envoi TIMESTAMP DEFAULT null ,
    date_validation timestamp DEFAULT null,
    date_delete timestamp DEFAULT null ,
    date_livrer timestamp DEFAULT null, 
    date_payer timestamp DEFAULT null ,
    remarque text default null
);

ALTER TABLE devis ADD FOREIGN KEY(id_client) REFERENCES client(id);

CREATE TABLE commande_devis(
    id bigserial PRIMARY KEY,
    id_devis bigint not null,
    id_categorie int NOT NULL ,
    produit_name VARCHAR(150) ,
    prix_euro decimal(18,2) CHECK( prix_euro > 0),
    prix_ariary decimal(18,2) CHECK( prix_ariary >= 0),
    couleur VARCHAR(50) ,
    taille VARCHAR(5) ,
    nombre int CHECK( nombre > 0 ) ,
    reference_site text,
    image text ,
    frais_importation_reel decimal(18,2) default null ,
    commission_reel decimal(18,2) default null
);

ALTER TABLE commande_devis ADD FOREIGN KEY (id_devis) REFERENCES devis(id);
ALTER TABLE commande_devis ADD FOREIGN KEY (id_categorie) REFERENCES categorie_produit(id);

CREATE TABLE vol_devis(
    id bigserial PRIMARY KEY ,
    id_devis bigint not null ,
    id_commande bigint not null ,
    quantiter int check ( quantiter > 0 ),
    id_vol int not null ,
    unique (id_commande, id_vol)
);

ALTER TABLE vol_devis ADD FOREIGN KEY (id_devis) REFERENCES devis(id);
ALTER TABLE vol_devis ADD FOREIGN KEY (id_commande) REFERENCES commande_devis(id);
ALTER TABLE vol_devis ADD FOREIGN KEY (id_vol) REFERENCES vol(id);

-----------------------------------------------------------
-----------------------caisse----------------------------
-----------------------------------------------------------


-- CREATE TABLE caisse_boutique(
--     id serial PRIMARY KEY ,
--     id_boutique int not NULL ,
--     montant decimal(18,2)
-- );
-- 
-- ALTER TABLE caisse_boutique ADD FOREIGN KEY (id_boutique) REFERENCES boutique(id);
-- 
-- CREATE TABLE caisse_devis(
--     id serial PRIMARY KEY ,
--     id_devis bigint not NULL ,
--     montant DECIMAL(18,2)
-- );
-- 
-- ALTER TABLE caisse_devis ADD FOREIGN KEY (id_devis) REFERENCES devis(id);


-----------------------------------------------------------
-----------------------PAYEMENTS----------------------------
-----------------------------------------------------------

CREATE TABLE type_payement(
    id serial PRIMARY KEY,
    nom VARCHAR(50) not null ,
    numero_resp VARCHAR(20) not null ,
    nom_num VARCHAR(50) not null ,
    nom_resp VARCHAR(50) not null 
);

----------------Devis--------------

CREATE TABLE payement_devis (
    id bigserial PRIMARY KEY ,
    id_devis bigint NOT NULL ,
    id_client bigint not null ,
    montant decimal(18,2) CHECK ( montant >=0 ) ,
    id_typepayement int NOT NULL ,
    date TIMESTAMP DEFAULT now() , 
    datepayement TIMESTAMP
);

ALTER TABLE payement_devis ADD FOREIGN KEY (id_devis) REFERENCES devis(id);
ALTER TABLE payement_devis ADD FOREIGN KEY (id_client) REFERENCES client(id);

CREATE TABLE adresse_livraisondevis(
    id bigserial PRIMARY KEY ,
    id_devis bigint not NULL ,
    id_commande bigint not null ,
    id_voldevis bigint not null ,
    quantiter int check ( quantiter > 0 ) ,
    ville VARCHAR(30) ,
    quartier VARCHAR(30) ,
    longitude double precision ,
    latitude double precision ,
    datepret timestamp default null ,
    dateestim timestamp default null ,
    isinboutique boolean default false ,
    fraislivraison decimal(18,2) ,
    date_livrer timestamp DEFAULT null
);

ALTER TABLE adresse_livraisondevis ADD FOREIGN KEY (id_devis) REFERENCES devis(id);
ALTER TABLE adresse_livraisondevis ADD FOREIGN KEY (id_commande) REFERENCES commande_devis(id);
ALTER TABLE adresse_livraisondevis ADD FOREIGN KEY (id_voldevis) REFERENCES vol_devis(id);

----------------Produits----------

CREATE TABLE facture(
    id bigserial PRIMARY key ,
    id_client bigint NOT null,
    id_boutique int NOT NULL,
    commission_ar decimal(10,2) ,
    date TIMESTAMP DEFAULT NOW() ,
    date_livrer Timestamp DEFAULT null,
    estpayer Timestamp DEFAULT null ,
    datesuspendue TIMESTAMP DEFAULT null
);

ALTER TABLE facture ADD FOREIGN KEY(id_client) REFERENCES client(id);
ALTER TABLE facture ADD FOREIGN KEY (id_boutique) REFERENCES boutique(id);

CREATE TABLE payement_produit(
    id bigserial PRIMARY KEY,
    id_facture bigint NOT NULL,
    id_preferenceproduit bigint NOT NULL ,
    id_typepayement int NOT NULL ,
    date TIMESTAMP DEFAULT NOW() ,
    dateLivrer TIMESTAMP DEFAULT null ,
    quantiter int check ( quantiter > 0 ) ,
    montant decimal(12,2) check ( montant >=0 ),
    prix_achat decimal(12,2) check ( prix_achat >=0 )
);

ALTER TABLE payement_produit ADD FOREIGN KEY (id_facture) REFERENCES facture(id);
ALTER TABLE payement_produit ADD FOREIGN KEY (id_preferenceproduit) REFERENCES preference_produit(id);
ALTER TABLE payement_produit ADD FOREIGN KEY (id_typepayement) REFERENCES type_payement(id);

CREATE TABLE adresse_livraisonfacture(
    id bigserial PRIMARY KEY ,
    id_facture bigint unique not NULL ,
    ville VARCHAR(30) ,
    quartier VARCHAR(30) ,
    longitude double precision ,
    latitude double precision ,
    datepret timestamp default null ,
    dateestim timestamp default null ,
    isinboutique boolean default false ,
    fraislivraison decimal(18,2)
);

ALTER TABLE adresse_livraisonfacture ADD FOREIGN KEY (id_facture) REFERENCES facture(id);

-----------------------------------------------------------
-----------------------EVENEMENTS----------------------------
----------------------------------------------------------- 

CREATE TABLE evenement(
    id bigserial PRIMARY KEY ,
    datedeb TIMESTAMP DEFAULT now() ,
    datefin TIMESTAMP NOT null ,
    description text
);

CREATE TABLE produit_event(
    id bigserial PRIMARY KEY ,
    id_evenement bigint not NULL ,
    id_categorie int NOT NULL ,
    produit_name VARCHAR(150) ,
    prix decimal(18,2) CHECK( prix > 0) ,
    couleur VARCHAR(50) ,
    taille VARCHAR(5) ,
    quantitermax int check ( quantitermax >0 ) ,
    reference_site text ,
    image text
);

ALTER TABLE produit_event ADD FOREIGN KEY (id_evenement) REFERENCES evenement(id);
ALTER TABLE produit_event ADD FOREIGN KEY (id_categorie) REFERENCES categorie_produit(id);

CREATE TABLE interaction_event(
    id bigserial PRIMARY KEY ,
    id_evenement bigint not NULL ,
    id_produitevent bigint not NULL ,
    id_client bigint not NULL ,
    quantiter int check ( quantiter >0 )
);

ALTER TABLE interaction_event ADD FOREIGN KEY (id_evenement) REFERENCES evenement(id);
ALTER TABLE interaction_event ADD FOREIGN KEY (id_produitevent) REFERENCES produit_event(id);
ALTER TABLE interaction_event ADD FOREIGN KEY (id_client) REFERENCES client(id);

CREATE TABLE type_depense(
    id bigserial primary key ,
    nom varchar(100) unique not null , 
    code varchar(10) unique not null
);

CREATE TABLE divers_depense(
    id bigserial primary key ,
    id_typedepense bigint not null ,
    corps text ,
    montant_ar decimal(18,2) check ( montant_ar > 0 ) ,
    date timestamp not null 
);

ALTER TABLE divers_depense ADD FOREIGN KEY (id_typedepense) REFERENCES type_depense(id); 

CREATE TABLE type_entrer(
    id bigserial primary key ,
    nom varchar(100) unique not null ,
    code varchar(10) unique not null
);

CREATE TABLE divers_entrer(
  id bigserial primary key ,
  id_typeentrer bigint not null ,
  corps text ,
  montant_ar decimal(18,2) check ( montant_ar > 0 ) ,
  date timestamp not null
);

ALTER TABLE divers_entrer ADD FOREIGN KEY (id_typeentrer) REFERENCES type_entrer(id);

---------------- COMMISSION +++
---------------- PDF devi, facture , liste rehetra 
---------------- export excel produit
---------------- AProvisionnement ( commande produit boutique ) 



